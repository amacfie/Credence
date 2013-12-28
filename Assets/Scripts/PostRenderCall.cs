//
// Copyright 2012-2014 by The Credence Game authors
//
// This file is part of The Credence Game.
//
// The Credence Game is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// The Credence Game is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with The Credence Game.  If not, see <http://www.gnu.org/licenses/>
//

using UnityEngine;
using System.Collections;

public class PostRenderCall : MonoBehaviour {

	void OnPostRender(){
		if(GameScript.singleton != null){
			GameScript.singleton.OnPostRender();
		}
	}
}
