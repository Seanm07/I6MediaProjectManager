/*
 * Last updated 2nd June 2017
 * Written by Sean McManus for i6 Media sean@i6.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Auth))]
public class AuthEditor : Editor {

	public static AuthEditor staticRef;

	private bool lastFacebookState;
	private bool lastTwitterState;
	private bool lastGoogleState;

	void Awake()
	{
		staticRef = this;

		#if auth
			SerializedObject serializedTarget = new SerializedObject (target);

			lastFacebookState = serializedTarget.FindProperty ("enableFacebookLoginAPI").boolValue;
			lastTwitterState = serializedTarget.FindProperty("enableTwitterLoginAPI").boolValue;
			lastGoogleState = serializedTarget.FindProperty("enableGooglePlayGamesAPI").boolValue;
		#endif
	}

	public static void RecalculateScriptingDefines()
	{
		staticRef.lastFacebookState = false;
		staticRef.lastTwitterState = false;
		staticRef.lastGoogleState = false;
	}

	public override void OnInspectorGUI()
	{
		#if auth
			SerializedObject serializedTarget = new SerializedObject (target);

			bool currentFacebookState = serializedTarget.FindProperty ("enableFacebookLoginAPI").boolValue;
			bool currentTwitterState = serializedTarget.FindProperty("enableTwitterLoginAPI").boolValue;
			bool currentGoogleState = serializedTarget.FindProperty("enableGooglePlayGamesAPI").boolValue;

			GUIStyle helpBoxStyle = GUI.skin.GetStyle("HelpBox");
			helpBoxStyle.padding = new RectOffset(5, 5, 5, 5);
			helpBoxStyle.richText = true;
			helpBoxStyle.wordWrap = true;

			if(currentFacebookState){
				GUILayout.Label("<color=#00ffff><size=12>Facebook API setup guide!</size></color>\n\n" + 
				"<color=#cccccc>Step 1:</color> Write this guide\n\n" + 
				"<color=#cccccc>Step 2:</color> If you see this text I forgot step 1", helpBoxStyle);
			}

			if(currentTwitterState){
				GUILayout.Label("<color=#00ffff><size=12>Twitter API setup guide!</size></color>\n\n" +
				"<color=#cccccc>Step 1:</color> Make sure <b>Installed Kits > Twitter</b> in the inspector of the file at <b>Assets/Editor Default Resources/FabricSettings</b> has the installation status set to <b>Imported</b> and untick <b>Installed</b> this will force it to ask for SDK keys again if set from a previous project in the past. <color=#dddddd>(If anything doesn't exist don't worry, just continue to step 2)</color>\n\n" + 
				"<color=#cccccc>Step 2:</color> Open the <b>Fabric > Prepare Fabric</b> window\n\n" + 
				"<color=#cccccc>Step 3:</color> If you see the 'Please select a kit to install' screen select <b>Twitter</b> otherwise if you see a login screen contact i6 Media and we'll guide you through login.\n\n" + 
				"<color=#cccccc>Step 4:</color> Click <b>Use Existing Account</b> and enter the <b>Twitter Key</b> and <b>Twitter Secret</b> given to you for this project (speak with i6 Media if you have not been given a key and we can set this up for you)\n\n<color=#cccccc>Step 4:</color> Click <b>Next</b> then <b>Apply</b>. If you already have a <b>FabricInit</b> and <b>TwitterInit</b> in your initial scene just press <b>Next</b> otherwise drag the blue box into the hierarchy. <color=#dddddd>(You should not have multiple of these objects in your hierarchy!)</color>", helpBoxStyle);

				GUILayout.Label("<color=#ff0000><size=12>Important note regarding a twitter auth error!</size></color>\n\n" + 
				"If you're getting <color=#dddddd>Must initialize Fabric before using singleton()</color> when trying to auth with twitter then you need to <b>redo the twitter setup!</b> It needs to re-modify some files which other plugins may have replaced!", helpBoxStyle);
			}

			if(currentGoogleState){
				GUILayout.Label("<color=#00ffff><size=12>Google Play Games API setup guide!</size></color>\n\n" +
				"<color=#cccccc>Step 1:</color> Open the <b>Window > Google Play Games > Setup > Android setup..</b> window\n\n" + 
				"<color=#cccccc>Step 2:</color> You should have been given the Android Resources to paste in. Note that if any changes are made to the leaderboard or achievements in the Google Play Console you will need to update the resources!\n\n" + 
				"<color=#cccccc>Step 3:</color> You should have been given the <b>Web App Client ID</b> paste this into  the <b>Client ID</b> field.\n\n" + 
				"<color=#cccccc>Step 4:</color> Click setup and you should be given a <b>Google Play Games configured successfully.</b> message after a few other dialog boxes <color=#dddddd>(if you don't get this message then either the resources or web app client ID are not for this game!)</color>", helpBoxStyle);

			GUILayout.Label("<color=#ff0000><size=12>Important note regarding crashing when attempting Google auth!</size></color>\n\n" + 
				"If the app is crashing when you attempt to Google auth then you need to <b>redo the Google Play Games API setup!</b> It needs to re-modify some files which other plugins may have replaced!", helpBoxStyle);
			}

			DrawDefaultInspector ();

			if (lastFacebookState != currentFacebookState) {
				ProjectManagerEditor.ModifyScriptingDefineSymbol (ProjectManagerEditor.ScriptingDefineSymbols.facebook, currentFacebookState);
				lastFacebookState = currentFacebookState;
			}

			if(lastTwitterState != currentTwitterState){
				ProjectManagerEditor.ModifyScriptingDefineSymbol(ProjectManagerEditor.ScriptingDefineSymbols.twitter, currentTwitterState);
				lastTwitterState = currentTwitterState;
			}

			if(lastGoogleState != currentGoogleState){
				ProjectManagerEditor.ModifyScriptingDefineSymbol(ProjectManagerEditor.ScriptingDefineSymbols.google, currentGoogleState);
				lastGoogleState = currentGoogleState;
			}
		#else
			EditorGUILayout.HelpBox("Enable 'Use Firebase Auth' from the Project Manager to use Auth!", MessageType.Info);
		#endif
	}
}