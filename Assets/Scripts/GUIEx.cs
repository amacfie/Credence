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
using System;

public class GUIEx {
	
	public static void LeftAligned(Action callback){
		AlignHorizontally(callback, false, true);
	}
	
	public static void Centered(Action callback){
		AlignHorizontally(callback, true, true);
	}
	
	public static void RightAligned(Action callback){
		AlignHorizontally(callback, true, false);
	}
	
	public static void AlignHorizontally(Action callback, bool leftSpace, bool rightSpace){
		GUILayout.BeginHorizontal();
		if(leftSpace) GUILayout.FlexibleSpace();
		callback();
		if(rightSpace) GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}
	
	public static void VCentered(Action callback){
		AlignVertically(callback, true, true);
	}
	
	public static void AlignVertically(Action callback, bool leftSpace, bool rightSpace){
		GUILayout.BeginVertical();
		if(leftSpace) GUILayout.FlexibleSpace();
		callback();
		if(rightSpace) GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
	}
}

