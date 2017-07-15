/*
 * Last updated 2nd June 2017
 * Written by Sean McManus for i6 Media sean@i6.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Database))]
public class DatabaseEditor : Editor {

	#if !database
	public override void OnInspectorGUI()
	{
		EditorGUILayout.HelpBox("Enable 'Use Firebase Database' from the Project Manager to use Database!", MessageType.Info);
	}
	#endif
}