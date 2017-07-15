/*
 * Last updated 2nd June 2017
 * Written by Sean McManus for i6 Media sean@i6.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Messaging))]
public class MessagingEditor : Editor {

	#if !messaging
	public override void OnInspectorGUI()
	{
		EditorGUILayout.HelpBox("Enable 'Use Firebase Messaging' from the Project Manager to use Cloud Messaging!", MessageType.Info);
	}
	#endif
}