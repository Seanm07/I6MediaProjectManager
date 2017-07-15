/*
 * Last updated 2nd June 2017
 * Written by Sean McManus for i6 Media sean@i6.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Invites))]
public class InvitesEditor : Editor {

	#if !invites
	public override void OnInspectorGUI()
	{
		EditorGUILayout.HelpBox("Enable 'Use Firebase Invites' from the Project Manager to use Invites!", MessageType.Info);
	}
	#endif
}