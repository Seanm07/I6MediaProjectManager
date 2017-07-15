/*
 * Last updated 2nd June 2017
 * Written by Sean McManus for i6 Media sean@i6.com
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

[System.Serializable]
public class PluginInfo {
	public List<PluginData> pluginInfo;
}

[System.Serializable]
public class PluginData {
	public string pluginName;
	public List<string> globalPathList;
	public List<string> iosPathList;
	public List<string> androidPathList;
}

[CustomEditor(typeof(ProjectManager))]
public class ProjectManagerEditor : Editor {

	private bool lastAdMobState, lastIasState, lastAnalyticsState, lastRemoteConfigState, lastAuthState, lastDatabaseState, lastInvitesState, lastMessagingState, lastStorageState;

	public enum ScriptingDefineSymbols
	{
		admob,
		ias,
		analytics,
		remote_config,
		auth,
		google,
		facebook,
		twitter,
		database,
		invites,
		messaging,
		storage
	}

	private static PluginInfo pluginDataStorage;

	void Awake()
	{
		SerializedObject serializedTarget = new SerializedObject (target);

		lastAdMobState = serializedTarget.FindProperty ("useAdMob").boolValue;
		lastIasState = serializedTarget.FindProperty ("useIAS").boolValue;
		lastAnalyticsState = serializedTarget.FindProperty ("useFirebaseAnalytics").boolValue;
		lastRemoteConfigState = serializedTarget.FindProperty ("useFirebaseRemoteConfig").boolValue;
		lastAuthState = serializedTarget.FindProperty ("useFirebaseAuth").boolValue;
		lastDatabaseState = serializedTarget.FindProperty ("useFirebaseDatabase").boolValue;
		lastInvitesState = serializedTarget.FindProperty ("useFirebaseInvites").boolValue;
		lastMessagingState = serializedTarget.FindProperty ("useFirebaseMessaging").boolValue;
		lastStorageState = serializedTarget.FindProperty ("useFirebaseStorage").boolValue;
	}

	void OnEnable()
	{
		SerializedObject serializedTarget = new SerializedObject (target);

		UpdatePluginList (false);

		serializedTarget.FindProperty("activeBuildTarget").enumValueIndex = (int)EditorUserBuildSettings.selectedBuildTargetGroup;
		serializedTarget.ApplyModifiedProperties (); 
	}

	public override void OnInspectorGUI()
	{
		SerializedObject serializedTarget = new SerializedObject (target);

		#if UNITY_4
			EditorGUILayout.HelpBox("These plugins do not officially support Unity 4! Make sure you know what you're doing as the plugins and scripts are untested in Unity 4!", MessageType.Warning);
		#endif

		bool adMobEnabled = serializedTarget.FindProperty ("useAdMob").boolValue;
		bool iasEnabled = serializedTarget.FindProperty ("useIAS").boolValue;
		bool analyticsEnabled = serializedTarget.FindProperty ("useFirebaseAnalytics").boolValue;
		bool remoteConfigEnabled = serializedTarget.FindProperty ("useFirebaseRemoteConfig").boolValue;
		bool authEnabled = serializedTarget.FindProperty ("useFirebaseAuth").boolValue;
		bool databaseEnabled = serializedTarget.FindProperty ("useFirebaseDatabase").boolValue;
		bool invitesEnabled = serializedTarget.FindProperty ("useFirebaseInvites").boolValue;
		bool messagingEnabled = serializedTarget.FindProperty ("useFirebaseMessaging").boolValue;
		bool storageEnabled = serializedTarget.FindProperty ("useFirebaseStorage").boolValue;

		if(!adMobEnabled) EditorGUILayout.HelpBox("The AdMob plugin is disabled! Unless told otherwise your project should always contain the admob plugin!", MessageType.Warning);
		if(!iasEnabled) EditorGUILayout.HelpBox("The IAS plugin is disabled! Unless told otherwise your project should always contain this plugin!", MessageType.Warning);
		if(!analyticsEnabled) EditorGUILayout.HelpBox("The Analytics plugin is disabled! Unless told otherwise your project should always contain this plugin!", MessageType.Warning);
		if(!remoteConfigEnabled) EditorGUILayout.HelpBox("The Remote Config plugin is disabled! We use this plugin to control some backend settings in your app! Unless told otherwise this plugin should be enabled!", MessageType.Warning);

		if (lastAdMobState != adMobEnabled) {
			ModifyScriptingDefineSymbol (ScriptingDefineSymbols.admob, adMobEnabled, true);
			lastAdMobState = adMobEnabled;
		}

		if (lastIasState != iasEnabled) {
			ModifyScriptingDefineSymbol (ScriptingDefineSymbols.ias, iasEnabled, true);
			lastIasState = iasEnabled;
		}

		if (lastAnalyticsState != analyticsEnabled) {
			if(!analyticsEnabled){
				// We need to first disable crashlytics properly so it removes crashlytics references from the Fabric manifest
				#if analytics
					Fabric.Internal.Crashlytics.Editor.CrashlyticsSetup.DisableCrashlytics();
				#endif
			}

			ModifyScriptingDefineSymbol (ScriptingDefineSymbols.analytics, analyticsEnabled, true);
			lastAnalyticsState = analyticsEnabled;

			if(analyticsEnabled){
				// Re-add crashlytics to the Fabric manifest
				#if analytics
					Fabric.Internal.Crashlytics.Editor.CrashlyticsSetup.EnableCrashlytics(true);
				#endif
			}
		}

		if (lastRemoteConfigState != remoteConfigEnabled) {
			ModifyScriptingDefineSymbol (ScriptingDefineSymbols.remote_config, remoteConfigEnabled, true);
			lastRemoteConfigState = remoteConfigEnabled; 
		}

		if (lastAuthState != authEnabled) {
			if (!authEnabled) {
				// Force remove the facebook define symbol if auth is disabled
				ModifyScriptingDefineSymbol (ScriptingDefineSymbols.facebook, false);

				// Force remove the twitter define symbol too
				ModifyScriptingDefineSymbol(ScriptingDefineSymbols.twitter, false);

				// Force remove the google define symbol too
				ModifyScriptingDefineSymbol(ScriptingDefineSymbols.google, false);
			} else {
				// This forces the facebook and twitter states to be recalculate when auth is enabled again
				AuthEditor.RecalculateScriptingDefines ();
			}

			// We need to disable auth after facebook and twitter or we'll have issues
			ModifyScriptingDefineSymbol (ScriptingDefineSymbols.auth, authEnabled, true);
			lastAuthState = authEnabled;
		}

		if (lastDatabaseState != databaseEnabled) {
			ModifyScriptingDefineSymbol (ScriptingDefineSymbols.database, databaseEnabled, true);
			lastDatabaseState = databaseEnabled;
		}

		if (lastInvitesState != invitesEnabled) {
			ModifyScriptingDefineSymbol (ScriptingDefineSymbols.invites, invitesEnabled, true);
			lastInvitesState = invitesEnabled;
		}

		if (lastMessagingState != messagingEnabled) {
			ModifyScriptingDefineSymbol (ScriptingDefineSymbols.messaging, messagingEnabled, true);
			lastMessagingState = messagingEnabled;
		}

		if (lastStorageState != storageEnabled) {
			ModifyScriptingDefineSymbol (ScriptingDefineSymbols.storage, storageEnabled, true);
			lastStorageState = storageEnabled;
		}

		if (GUILayout.Button ("Force hide progress bar")) {
			OnScriptsReloaded ();
		}

		if (GUILayout.Button ("Recalculate Scripting Defines (Not usually needed)")) {
			ClearScriptingDefineSymbols ();

			lastAdMobState = lastAnalyticsState = lastAuthState = lastDatabaseState = lastIasState = lastInvitesState = lastMessagingState = lastRemoteConfigState = lastStorageState = false;
			AuthEditor.RecalculateScriptingDefines ();
			ModifyPlugins ("firebase_global", true, false);
			Debug.Log ("Scripting defines recalculated! Note: Any custom set defines need to be set again!");
		}

		if (GUILayout.Button ("Update plugin file list (Reads plugins from PluginImportData.json)")) {
			UpdatePluginList (true);
		}

		DrawDefaultInspector ();
	}

	private void UpdatePluginList(bool DoLogs)
	{
		// The developer might have moved PluginImportData.json so search for it instead of using a fixed path
		string[] searchResults = AssetDatabase.FindAssets("PluginImportData");
		string path = "";

		if (searchResults.Length > 0) {
			// Just use the first path found and complain if there's multiple
			path = AssetDatabase.GUIDToAssetPath(searchResults [0]);

			if (searchResults.Length > 1)
				ProjectManager.AdvLog ("Multiple PluginImportData files found, make sure to delete any duplicates! (Using: " + path + ")");
		} else {
			Debug.LogError ("Your project is missing PluginImportData.json! Your project will probably fail to build!");
			return;
		}

		StreamReader reader = new StreamReader (path);

		pluginDataStorage = JsonUtility.FromJson<PluginInfo>(reader.ReadToEnd ());

		if (DoLogs) {
			for (int i = 0; i < pluginDataStorage.pluginInfo.Count; i++) {
				for (int globalPathId = 0; globalPathId < pluginDataStorage.pluginInfo [i].globalPathList.Count; globalPathId++)
					ProjectManager.AdvLog (pluginDataStorage.pluginInfo [i].pluginName + " - globalPathList[" + globalPathId + "] - " + pluginDataStorage.pluginInfo [i].globalPathList [globalPathId]);

				for (int iosPathId = 0; iosPathId < pluginDataStorage.pluginInfo [i].iosPathList.Count; iosPathId++)
					ProjectManager.AdvLog (pluginDataStorage.pluginInfo [i].pluginName + " - iosPathList[" + iosPathId + "] - " + pluginDataStorage.pluginInfo [i].iosPathList [iosPathId]);

				for (int androidPathId = 0; androidPathId < pluginDataStorage.pluginInfo [i].iosPathList.Count; androidPathId++)
					ProjectManager.AdvLog (pluginDataStorage.pluginInfo [i].pluginName + " - androidPathList[" + androidPathId + "] - " + pluginDataStorage.pluginInfo [i].iosPathList [androidPathId]);
			}

			Debug.Log ("Finished reloading plugin list!");
		}
	}

	public static void ClearScriptingDefineSymbols()
	{
		PlayerSettings.SetScriptingDefineSymbolsForGroup (EditorUserBuildSettings.selectedBuildTargetGroup, "");
	}

	[DidReloadScripts]
	private static void OnScriptsReloaded()
	{
		EditorUtility.ClearProgressBar (); 
	}

	/// <summary>
	/// Returns a string list of plugin files
	/// </summary>
	/// <returns>The plugin files.</returns>
	/// <param name="pluginName">Plugin name</param>
	/// <param name="buildTarget">Build target</param>
	/// <param name="returnRequested">Returns the files for the requested build target if <c>true</c> otherwise it'll return all the files we don't need (so we can remove these files etc)</param>
	private static List<string> GetPluginFiles(string pluginName, BuildTargetGroup buildTarget, bool returnRequested = true)
	{
		List<string> returnPluginList = new List<string> ();

		for (int i = 0; i < pluginDataStorage.pluginInfo.Count; i++) {
			if (pluginDataStorage.pluginInfo [i].pluginName == pluginName) {
				if (returnRequested) {
					foreach (string globalPathItem in pluginDataStorage.pluginInfo [i].globalPathList)
						returnPluginList.Add (globalPathItem);

					switch (buildTarget) {
						case BuildTargetGroup.Android:
							foreach (string androidPathItem in pluginDataStorage.pluginInfo[i].androidPathList)
								returnPluginList.Add (androidPathItem);
							break;

						case BuildTargetGroup.iOS:
							foreach (string iosPathItem in pluginDataStorage.pluginInfo[i].iosPathList)
								returnPluginList.Add (iosPathItem);
							break;
					}
				} else {
					switch (buildTarget) {
						case BuildTargetGroup.Android:
							foreach (string iosPathItem in pluginDataStorage.pluginInfo[i].iosPathList)
								returnPluginList.Add (iosPathItem);
							break;

						case BuildTargetGroup.iOS:
							foreach (string androidPathItem in pluginDataStorage.pluginInfo[i].androidPathList)
								returnPluginList.Add (androidPathItem);
							break;
					}
				}
			}
		}

		return returnPluginList;
	}

	public static bool IsScriptingDefineSymbolActive(ScriptingDefineSymbols defineSymbol)
	{
		string activeDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup (EditorUserBuildSettings.selectedBuildTargetGroup);

		return activeDefineSymbols.Contains (defineSymbol.ToString ());
	}

	public static void DisplayProgressBar(bool doAdd, bool dependenciesCall, string value)
	{
		string[] enableWords = {"Cramming in", "Stuffing in", "Spawning", "Suiting up", "Wedging in", "Probably breaking everything by adding ", "Lets hope you can build with", "Enabling", "Adding"};
		string[] disableWords = {"Hoovering up", "Evicting", "Lasering", "Shunning", "Inb4 you can't build without", "Removing", "Disabling"};
		string[] dependencyWords = { "Fixing dependencies for", "Repairing dependencies for", "Cleaning up depencencies for" };

		EditorUtility.DisplayProgressBar (dependenciesCall ? "Cleaning up dependencies of " : (doAdd ? "Adding" : "Removing") + " Plugins..", (dependenciesCall ? dependencyWords[UnityEngine.Random.Range(0, dependencyWords.Length-1)] : (doAdd ? enableWords[UnityEngine.Random.Range(0, enableWords.Length-1)] : disableWords[UnityEngine.Random.Range(0, enableWords.Length-1)])) + " " + value + ".. Hold on \ud83d\udd28", 1f);
	}

	public static void ModifyScriptingDefineSymbol(ScriptingDefineSymbols defineSymbol, bool doAdd, bool userAction = false, bool dependenciesCall = false)
	{
		string value = defineSymbol.ToString ();

		// This must be done before any ModifyPlugins calls or we'll end up displaying the main action AFTER fixing dependencies which is reversed
		if (!doAdd && (EditorApplication.isCompiling || EditorApplication.isUpdating)) {
			DisplayProgressBar (doAdd, dependenciesCall, value);
		}

		// If we're adding the plugin then do it before we set the scripting define symbol
		// (or script errors will be thrown as scripts interact with the plugin again)
		if(doAdd) ModifyPlugins (value, doAdd, userAction);

		string newScriptingDefines = "";

		newScriptingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup (EditorUserBuildSettings.selectedBuildTargetGroup);

		if (doAdd) {
			// Only add it if it doesn't already exist
			if(!newScriptingDefines.Contains(value))
				newScriptingDefines += ";" + value; // If the ; isn't required Unity automatically removes it for us :)
		} else {
			newScriptingDefines = newScriptingDefines.Replace (value, ""); // Unity will automatically remove any left over ; separators for us
		}

		PlayerSettings.SetScriptingDefineSymbolsForGroup (EditorUserBuildSettings.selectedBuildTargetGroup, newScriptingDefines);

		// If we're removing the plugin then do it once the scripting define symbol has been removed
		// (so we don't get script errors complaining about the plugin being missing before the scripts are disabled)
		if(!doAdd) ModifyPlugins (value, doAdd, userAction);
	}

	public static void MoveFile (string fromPath, string toPath)
	{
		ProjectManager.AdvLog ("Moving file from " + fromPath + " to " + toPath);

		// Are we moving a directory or just a file?
		bool isDir = File.GetAttributes (fromPath) == FileAttributes.Directory;

		// Check to make sure a file doesn't already exist at the toPath
		if (!isDir) {
			if (File.Exists (fromPath)) {
				if (!File.Exists (toPath)) {
					File.Move(fromPath, toPath);
				} else {
					// Plugins probably alread moved.. however there are duplicates still inside the project!!
					ProjectManager.AdvLog ("Path to move plugins into already exists at " + toPath);

					// Delete the existing file
					File.Delete(fromPath);
					ProjectManager.AdvLog ("Existing file at " + fromPath + " has been deleted!");
				}
			} else {
				// Plugins probably already moved.. we just have to hope they're in the toPath from here or they're lost and need reimporting
				ProjectManager.AdvLog ("Path to move plugins from doesn't exist at " + fromPath);
			}
		} else {
			if (Directory.Exists (fromPath)) {
				if (!Directory.Exists (toPath)) {
					Directory.Move(fromPath, toPath);
				} else {
					// Plugins probably already moved.. however there are duplicates still inside the project!!
					ProjectManager.AdvLog ("Path to move plugins into already exists at " + toPath);

					// Delete the existing directory
					Directory.Delete(fromPath, true);
					ProjectManager.AdvLog ("Existing folder at " + fromPath + " has been deleted!");
				}
			} else {
				// Plugins probably already moved.. we just have to hope they're in the toPath from here or they're lost and need reimporting
				ProjectManager.AdvLog ("Path to move plugins from doesn't exist at " + fromPath);
			}
		}
	}

	private static void ModifyPlugins (string value, bool doAdd, bool userAction = false)
	{
		ProjectManager.AdvLog ("Modifying plugins: " + value + ", " + doAdd + ", " + userAction);

		// If this is an add action this must be done before any scripting defines are changed
		if (doAdd && (EditorApplication.isCompiling || EditorApplication.isUpdating)) {
			DisplayProgressBar (doAdd, false, value);
		}

		// Move plugins not in use outside of the assets folder
		// (I was just going to change their import platform settings but it was a pain to manage them for different platforms 
		// so just keeping defaults and moving out of the assets folder will be fine + cleaner project with less projects active)
		List<string> activePlatformPluginFiles = GetPluginFiles (value, EditorUserBuildSettings.selectedBuildTargetGroup, true);
		List<string> otherPlatformPluginFiles = GetPluginFiles (value, EditorUserBuildSettings.selectedBuildTargetGroup, false);

		string projectPath = Application.dataPath.Replace ("/Assets", ""); // Absolute path to the project with Assets folder removed e.g C:/Projects/My Game
		string disabledPluginsPath = projectPath + "/Disabled Plugins"; // C:/Projects/My Game/Disabled Plugins

		// Make sure the Disabled Plugins folder exists
		if (!Directory.Exists (disabledPluginsPath)) {
			Directory.CreateDirectory (disabledPluginsPath);
		}

		if (Directory.Exists (disabledPluginsPath)) {
			// Move plugins files for other platforms out of the project
			if (otherPlatformPluginFiles.Count > 0) {

				ProjectManager.AdvLog ("There are " + otherPlatformPluginFiles.Count + " other plugin files to move out of the project!");

				foreach (string file in otherPlatformPluginFiles) {
					ProjectManager.AdvLog ("Moving " + file + " out of project!");

					string insideProjectPath = "Assets/" + file;
					string outsideProjectPath = file; 

					// Run GetAttributes within a try catch or if the file doesn't exist it'll throw an error (and we're calling this in the first place to see if the file exists ;_;)
					try {
						// Are we moving a directory or just a file? 
						bool isDir = File.GetAttributes (projectPath + "/" + insideProjectPath) == FileAttributes.Directory;

						// Before we bother calculating any moving or folder creation check if the files we want to move out of the project are actually in the project already
						if (!isDir) {
							if (File.Exists (projectPath + "/" + insideProjectPath)) {
								// Get each folder as an array from the new file path so we can create all the nessesary folders
								string[] splitOutsidePath = outsideProjectPath.Split ("/" [0]); // ["Plugins", "Android", "Example.aar"]
								string curPath = "";

								// Minus 1 from the total iteration because the last value will be the actual file
								for (int i = 0; i < splitOutsidePath.Length - 1; i++) {
									curPath += "/" + splitOutsidePath [i];

									if (!Directory.Exists (disabledPluginsPath + curPath)) {
										Directory.CreateDirectory (disabledPluginsPath + curPath);
									}
								}

								// All the folders should now be created and the files can be moved
								MoveFile (projectPath + "/" + insideProjectPath, disabledPluginsPath + "/" + outsideProjectPath);
								MoveFile (projectPath + "/" + insideProjectPath + ".meta", disabledPluginsPath + "/" + outsideProjectPath + ".meta");
							} else {
								ProjectManager.AdvLog ("File to be taken out of project not found at: " + projectPath + "/" + insideProjectPath);
							}
						} else {
							if (Directory.Exists (projectPath + "/" + insideProjectPath)) {
								// Get each folder as an array from the new file path so we can create all the nessesary folders
								string[] splitOutsidePath = outsideProjectPath.Split ("/" [0]); // ["Plugins", "Android", "Example"]
								string curPath = "";

								// Minus 1 from the total iteration because the last value will be the actual folder we're moving
								for (int i = 0; i < splitOutsidePath.Length - 1; i++) {
									curPath += "/" + splitOutsidePath [i];

									if (!Directory.Exists (disabledPluginsPath + curPath)) {
										Directory.CreateDirectory (disabledPluginsPath + curPath);
									}
								}

								// All the folders should now be created and we can move the wanted directories into them
								MoveFile (projectPath + "/" + insideProjectPath, disabledPluginsPath + "/" + outsideProjectPath);
								MoveFile (projectPath + "/" + insideProjectPath + ".meta", disabledPluginsPath + "/" + outsideProjectPath + ".meta");
							} else {
								ProjectManager.AdvLog ("Directory to be moved out of project not found at: " + projectPath + "/" + insideProjectPath);
							}
						}
					} catch (System.Exception error) {
						ProjectManager.AdvLog ("File or folder not found! " + error.Message);
					}
				}
			}

			// Move the plugin files for the active platform
			ProjectManager.AdvLog ("The activePlatformPluginFiles has " + activePlatformPluginFiles.Count + " files to move");

			if (activePlatformPluginFiles.Count > 0) {
				foreach (string file in activePlatformPluginFiles) {
					ProjectManager.AdvLog ("Yay lets move " + file);

					// These variables are only relative to the assets folder (commented values are flipped if doAdd is false)
					string oldFullFilePath = (doAdd ? "Disabled Plugins/" : "Assets/") + file; // (if doAdd) Disabled Plugins/Plugins/Android/Example.aar
					string newFullFilePath = (doAdd ? "Assets/" : "Disabled Plugins/") + file; // (if doAdd) Assets/Plugins/Android/Example.aar

					// Run GetAttributes within a try catch or if the file doesn't exist it'll throw an error (and we're calling this in the first place to see if the file exists ;_;)
					try {
						// Are we moving to a directory or just a file?
						bool isDir = File.GetAttributes (projectPath + "/" + oldFullFilePath) == FileAttributes.Directory;

						// Before we bother calculating any moving or folder creation make sure the file we're wanting to move exist
						if (!isDir) {
							if (File.Exists (projectPath + "/" + oldFullFilePath)) {
								if (!doAdd) {
									newFullFilePath = newFullFilePath.Replace ("Disabled Plugins/", ""); // Plugins/Android/Example.aar

									// Get each folder as an array from the new file path so we can create all the nessesary folders
									string[] splitNewPaths = newFullFilePath.Split ("/" [0]); // ["Plugins", "Android", "Example.aar"]
									string curPath = "";

									// Minus 1 from the total iteration because the last value will be the actual file
									for (int i = 0; i < splitNewPaths.Length - 1; i++) {
										curPath += "/" + splitNewPaths [i];

										if (!Directory.Exists (disabledPluginsPath + curPath)) {
											Directory.CreateDirectory (disabledPluginsPath + curPath);
										}
									}

									// All the folders should now be created and the files can be moved
									MoveFile (projectPath + "/" + oldFullFilePath, disabledPluginsPath + "/" + newFullFilePath);
									MoveFile (projectPath + "/" + oldFullFilePath + ".meta", disabledPluginsPath + "/" + newFullFilePath + ".meta");
								} else {
									MoveFile (projectPath + "/" + oldFullFilePath, projectPath + "/" + newFullFilePath);
									MoveFile (projectPath + "/" + oldFullFilePath + ".meta", projectPath + "/" + newFullFilePath + ".meta");
								}
							} else {
								ProjectManager.AdvLog ("File to be moved doesn't exist! (" + oldFullFilePath + ")");
							}
						} else {
							if(Directory.Exists(projectPath + "/" + oldFullFilePath)){
								if(!doAdd){
									newFullFilePath = newFullFilePath.Replace("Disabled Plugins/", ""); // Plugins/Android/Example

									// Get each folder as an array from the new file path so we can create all the nessesary folders
									string[] splitNewPaths = newFullFilePath.Split("/" [0]); // ["Plugins", "Android", "Example"]
									string curPath = "";

									// Minus 1 from the total iteration because the last value will be the actual folder
									for(int i=0;i < splitNewPaths.Length - 1;i++){
										curPath += "/" + splitNewPaths[i];

										if(!Directory.Exists(disabledPluginsPath + curPath)){
											Directory.CreateDirectory(disabledPluginsPath + curPath);
										}
									}

									// All the folders should now be created and the folder can be moved
									MoveFile(projectPath + "/" + oldFullFilePath, disabledPluginsPath + "/" + newFullFilePath);
									MoveFile(projectPath + "/" + oldFullFilePath + ".meta", disabledPluginsPath + "/" + newFullFilePath + ".meta");
								} else {
									MoveFile(projectPath + "/" + oldFullFilePath, projectPath + "/" + newFullFilePath);
									MoveFile(projectPath + "/" + oldFullFilePath + ".meta", projectPath + "/" + newFullFilePath + ".meta");
								}
							} else {
								ProjectManager.AdvLog ("File to be moved doesn't exist! (" + oldFullFilePath + ")");
							}
						}
					} catch (System.Exception error) {
						ProjectManager.AdvLog ("File or folder not found! " + error.Message);
					}
				}
			} else {
				ProjectManager.AdvLog ("This platform does not have any plugin files for " + value + "!");
			}
		} else {
			ProjectManager.AdvLog ("Failed to create Disabled Plugins folder or it was deleted whilst moving files!");
		}

		// If this was a request to remove a plugin we now need to force re-add any files which still active plugins shared!
		if (!doAdd) {
			string[] ScriptingDefineSymbolNames = System.Enum.GetNames (typeof(ScriptingDefineSymbols));

			for (int i = 0; i < ScriptingDefineSymbolNames.Length; i++) {
				ScriptingDefineSymbols curScriptingDefineSymbol = (ScriptingDefineSymbols)i;

				if(IsScriptingDefineSymbolActive(curScriptingDefineSymbol))
					ModifyScriptingDefineSymbol(curScriptingDefineSymbol, true, userAction, true);
			}
		}

		// Force reload assets otherwise it'll wait for the user to click the Unity window again
		AssetDatabase.Refresh(ImportAssetOptions.Default);
	}
}