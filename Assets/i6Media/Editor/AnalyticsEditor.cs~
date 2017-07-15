/*
 * Last updated 2nd June 2017
 * Written by Sean McManus for i6 Media sean@i6.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Analytics))]
public class AnalyticsEditor : Editor {

	#if !analytics
	public override void OnInspectorGUI()
	{
		EditorGUILayout.HelpBox("Enable 'Use Firebase Analytics' from the Project Manager to use Analytics!", MessageType.Info);
	}
	#endif
}