using UnityEngine;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

// Responsibilities:
// * Read QuestionGenerators from XML.
// * Shuffle questions, don't choose completely randomly.
// * Respect tags.
// * Respect per-generator frequency adjustments.
//
public class QuestionsScript : MonoBehaviour {
	
	public enum TagUsage { None, Some, All, COUNT };
	public enum GeneratorFrequency { Rare = 0, Default = 1, Frequent = 2 };

	const string SUBTAG_DELIMITER = "::";
	public static QuestionsScript singleton;
	public List<QuestionGenerator> generators;

	// Options set for particular question tags.
	public static SortedDictionary<string, TagUsage> tags;

	public static Dictionary<string, GeneratorFrequency> generatorFrequency;
	
	public GameObject gamePrefab;
	
	private List<QuestionGenerator> generatorShuffle;
	private List<QuestionGenerator>.Enumerator shuffleIndex;

	private bool loadedDatabases = false;
	private bool loadedQuestions = false;
	
	// Use this for initialization
	void Awake () {
		singleton = this;
		tags = new SortedDictionary<string, TagUsage>();
		generatorFrequency = new Dictionary<string, GeneratorFrequency>();

		StartCoroutine(LoadAllQuestionsCoroutine());
	}
	
	IEnumerator LoadAllQuestionsCoroutine(){
		LoadAllQuestions();
		Instantiate(gamePrefab);
		loadedQuestions = true;
		yield break;
	}
	
	public void LoadAllQuestions(){
		generators = new List<QuestionGenerator>();

		QuestionDatabase.databases = QuestionDatabase.LoadDatabases();
		loadedDatabases = true;
		
		bool existsGoodDatabase = false;
		foreach(QuestionDatabase database in QuestionDatabase.databases){
			if(database.downloaded && database.used){
				LoadQuestions(database);
				existsGoodDatabase = true;
			}
		}
		if(!existsGoodDatabase){
			foreach(QuestionDatabase database in QuestionDatabase.databases){
				if(database.downloaded){
					Debug.LogWarning("Since no good database has been found, " + database.name + " has been enabled.");
					database.used = true;
					break;
				}
			}
		}
		
		GeneratorsUpdated();
	}

	private void GeneratorsUpdated(){
		// Add tags that are not yet known
		foreach(QuestionGenerator generator in generators){
			foreach(string tag in generator.m_tags){
				if(!tags.ContainsKey(tag)){
					tags[tag] = TagUsage.Some;
				}
				if(HasSubTag(tag)){
					string parent = GetParentTag(tag);
					if(!tags.ContainsKey(parent)){
						tags[parent] = TagUsage.Some;
					}
				}
			}
		}

		SelectAndShuffleGenerators();
	}
	
	public Question GenerateQuestion(){
		if(generatorShuffle == null || !shuffleIndex.MoveNext()){
			SelectAndShuffleGenerators();
		}
		QuestionGenerator generator = shuffleIndex.Current;
		return generator.GenerateQuestion();
	}

	public int GetActiveGeneratorCount(){
		return generators.FindAll(generator => generator.m_active).Count;
	}
	
	private void LoadQuestions(QuestionDatabase database){
		try {
			using(XmlReader reader = XmlReader.Create(database.FilePath)){
				reader.ReadToDescendant("QuestionGenerator");
				do {
					QuestionGenerator generator = QuestionGenerator.CreateGeneratorFromXml(reader);
					if(generator != null){
						generator.m_database = database;
						generators.Add(generator);
					}
				} while(reader.ReadToNextSibling("QuestionGenerator"));
			}
		} catch {
			database.used = false;
			database.downloaded = false;
		}
	}

	public void LoadQuestionsAndUpdate(QuestionDatabase database){
		LoadQuestions(database);
		GeneratorsUpdated();
	}
	
	public void SelectAndShuffleGenerators(){
		// Tags may have changed. Recompute which generators are active.
		foreach(QuestionGenerator generator in generators){
			generator.m_active = IsGeneratorActive(generator);
		}
		int activeCount = GetActiveGeneratorCount();

		// We want to select approximately desiredQuestionCount generators from
		// the list and we do it by giving each generator a chance to be in the
		// current set. This has the advantage of each generator being used at
		// most once per set.
		// Note that baseQuestionProbability can be larger than 1. That's fine.
		double desiredQuestionCount = 10.0;
		double baseQuestionProbability = desiredQuestionCount / activeCount;

		generatorShuffle = new List<QuestionGenerator>();
		while(generatorShuffle.Count == 0){
			foreach(QuestionGenerator generator in generators){
				if(!generator.m_active)
					continue;

				double probability = baseQuestionProbability * generator.m_weight;
				switch(GetGeneratorFrequency(generator.m_id)){
				case GeneratorFrequency.Rare:
					probability /= 4.0;
					break;
				case GeneratorFrequency.Frequent:
					probability *= 4.0;
					break;
				case GeneratorFrequency.Default:
					break;
				}

				if (Random.value < probability)
					generatorShuffle.Insert(Random.Range(0, generatorShuffle.Count+1), generator);
			}
		}

		shuffleIndex = generatorShuffle.GetEnumerator();
		shuffleIndex.MoveNext();
	}

	private static bool IsGeneratorActive(QuestionGenerator generator){
		bool active = true;
		foreach(string tag in generator.m_tags){
			if(HasSubTag(tag)){
				string parent = GetParentTag(tag);
				if(tags[parent] == TagUsage.All){
					return true;
				}
				else if(tags[parent] == TagUsage.None){
					active = false;
				}
			}
			if(tags[tag] == TagUsage.All){
				return true;
			} else if(tags[tag] == TagUsage.None){
				active = false;
			}
		}
		return active;
	}
	
	void OnGUI(){
		if(loadedQuestions) return;
		if(!loadedDatabases){
			GUI.Label(new Rect(15f, 15f, Screen.width, Screen.height), "Loading question databases...");
			return;
		}
		GUI.Label(new Rect(15f, 15f, Screen.width, Screen.height), "Found " + QuestionDatabase.databases.Count + " question databases.");
		GUI.Label(new Rect(15f, 15f, Screen.width, Screen.height), "Loading questions...");
	}
	
	public static bool HasSubTag(string tag){
		return tag.IndexOf(SUBTAG_DELIMITER) >= 0;
	}
	
	public static string GetParentTag(string tag){
		return tag.Substring(0, tag.IndexOf(SUBTAG_DELIMITER));
	}
	
	public static string GetChildTag(string tag){
		return tag.Substring(tag.IndexOf(SUBTAG_DELIMITER) + SUBTAG_DELIMITER.Length);
	}

	public static GeneratorFrequency GetGeneratorFrequency(string id){
		if(generatorFrequency.ContainsKey(id)){
			return generatorFrequency[id];
		}
		return GeneratorFrequency.Default;
	}

	public static void SetGeneratorFrequency(string id, GeneratorFrequency freq){
		if(freq == GeneratorFrequency.Default){
			generatorFrequency.Remove(id);
		} else {
			generatorFrequency[id] = freq;
		}
	}
}
