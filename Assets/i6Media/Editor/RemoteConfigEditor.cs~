/*
 * Last updated 2nd June 2017
 * Written by Sean McManus for i6 Media sean@i6.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RemoteConfig))]
public class RemoteConfigEditor : Editor {

	#if !remote_config
	public override void OnInspectorGUI()
	{
		EditorGUILayout.HelpBox("Enable 'Use Firebase Remote Config' from the Project Manager to use Remote Config!", MessageType.Info);
	}
	#endif
}