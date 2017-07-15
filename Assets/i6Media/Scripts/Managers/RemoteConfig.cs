/*
 * Last updated 2nd June 2017
 * Written by Sean McManus for i6 Media sean@i6.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if remote_config
	// If you've just updated firebase and it's complaining Firebase.RemoteConfig doesn't exist 
	// then make sure Firebase.RemoteConfig.dll has Editor, iOS and Android as its included platforms
	using Firebase.RemoteConfig;
#endif

public class CustomRemoteConfig {
	public string name;
	public object defaultValue;
}

public class RemoteConfig : MonoBehaviour {

	private static RemoteConfig staticRef;

	public List<CustomRemoteConfig> customDefaultConfig = new List<CustomRemoteConfig> ();

	#pragma warning disable 0414 0067 // Supress the 'is never used' warning when remote config is disabled
		public static event Action OnRemoteConfigReady;
	#pragma warning restore 0414 0067

	void Awake()
	{
		staticRef = (staticRef == null ? this : staticRef);
	}

	public static void Initialize()
	{
		#if remote_config
			ProjectManager config = ProjectManager.staticRef;

			Dictionary<string, object> defaultConfig = new Dictionary<string, object>();

			defaultConfig.Add ("useAdMob", config.useAdMob);
			defaultConfig.Add ("useIAS", config.useIAS);
			defaultConfig.Add ("useFirebaseAnalytics", config.useFirebaseAnalytics);
			//defaultConfig.Add ("useFirebaseRemoteConfig", config.useFirebaseRemoteConfig);

			defaultConfig.Add ("useFirebaseAuth", config.useFirebaseAuth);
			defaultConfig.Add ("useFirebaseDatabase", config.useFirebaseDatabase);
			defaultConfig.Add ("useFirebaseInvites", config.useFirebaseInvites);
			defaultConfig.Add ("useFirebaseMessaging", config.useFirebaseMessaging);
			defaultConfig.Add ("useFirebaseStorage", config.useFirebaseStorage);

			defaultConfig.Add ("isDebugModeActive", config.isDebugModeActive);

			// Add custom developer defined values from the inspector into the defaultConfig dictionary
			foreach (CustomRemoteConfig newDefaultConfig in staticRef.customDefaultConfig)
				defaultConfig.Add (newDefaultConfig.name, newDefaultConfig.defaultValue);

			// Assign all the default values from above
			FirebaseRemoteConfig.SetDefaults (defaultConfig);

			staticRef.FetchRemoteConfig ();
		#endif
	}

	public void FetchRemoteConfig()
	{
		#if remote_config
			FirebaseRemoteConfig.FetchAsync ().ContinueWith (task => {
				if(task.IsCanceled){
					Analytics.LogError ("Firebase Fetch RemoteConfig", "Fetching of RemoteConfig canceled!");
					return;
				}

				if(task.IsFaulted){
					Analytics.LogError ("Firebase Fetch RemoteConfig", "Error: " + task.Exception.Message);
					return;
				}

				FlushCache();
			});
		#endif
	}

	// This needs to be called when a value is expected to be different or the GetValue(..) values will be cached
	public static void FlushCache()
	{
		#if remote_config
			FirebaseRemoteConfig.ActivateFetched ();

			staticRef.RemoteConfigReady ();
		#endif
	}

	public void RemoteConfigReady()
	{
		#if remote_config
			ProjectManager config = ProjectManager.staticRef;

			// Update the ProjectManager values (reflection COULD be used to cut down the code but for performance reasons I'm just manually setting each variable)
			config.useAdMob = GetBoolValue("useAdMob") ? true : config.useAdMob;
			config.useIAS = GetBoolValue ("useIAS") ? true : config.useIAS;
			config.useFirebaseAnalytics = GetBoolValue ("useFirebaseAnalytics") ? true : config.useFirebaseAnalytics;
			//config.useFirebaseRemoteConfig = GetBoolValue ("useFirebaseRemoteConfig");

			config.useFirebaseAuth = GetBoolValue ("useFirebaseAuth") ? true : config.useFirebaseAuth;
			config.useFirebaseDatabase = GetBoolValue ("useFirebaseDatabase") ? true : config.useFirebaseDatabase;
			config.useFirebaseInvites = GetBoolValue ("useFirebaseInvites") ? true : config.useFirebaseInvites;
			config.useFirebaseMessaging = GetBoolValue ("useFirebaseMessaging") ? true : config.useFirebaseMessaging;
			config.useFirebaseStorage = GetBoolValue ("useFirebaseStorage") ? true : config.useFirebaseStorage;
			config.isDebugModeActive = GetBoolValue ("isDebugModeActive") ? true : config.isDebugModeActive;

			OnRemoteConfigReady ();
		#endif
	}

	#if remote_config
		public static ConfigValue GetValue(string key)
		{
			return FirebaseRemoteConfig.GetValue (key);
		}

		public static string GetStringValue(string key)
		{
			return GetValue (key).StringValue;
		}

		public static bool GetBoolValue(string key)
		{
			return GetValue (key).BooleanValue;
		}

		public static double GetDoubleValue(string key)
		{
			return GetValue (key).DoubleValue;
		}

		public static long GetLongValue(string key)
		{
			return GetValue (key).LongValue;
		}

		// Note that any values outside the int range will be clamped due to limits
		public static int GetIntValue(string key)
		{
			return (int)GetValue (key).LongValue;
		}
	#else
		public static void GetValue(string key){ }
		public static string GetStringValue(string key){ return string.Empty; }
		public static bool GetBoolValue(string key){ return false; }
		public static double GetDoubleValue(string key){ return 0D; }
		public static long GetLongValue(string key){ return 0L; }
		public static int GetIntValue(string key){ return 0; }
	#endif
}
