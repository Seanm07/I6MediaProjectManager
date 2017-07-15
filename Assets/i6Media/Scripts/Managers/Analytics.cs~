/*
 * Last updated 2nd June 2017
 * Written by Sean McManus for i6 Media sean@i6.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if analytics
	using Firebase.Analytics;
	using Fabric.Crashlytics;
#endif

public class Analytics : MonoBehaviour {

	// Enable or disable analytics collection (enabled by default)
	public static void SetAnalyticsEnabled(bool wantEnabled)
	{
		#if analytics
			FirebaseAnalytics.SetAnalyticsCollectionEnabled (wantEnabled);
		#endif
	}

	/* 
	 * Example of event logging, keeping events categorised:
	 * LogEvent("Car Selection", "Colour", "Blue");
	 * LogEvent("Car Selection", "Colour", "Red");
	 * LogEvent("Car Selection", "Model", "Ford");
	 * LogEvent("Car Selection", "Gear Type", "Manual");
	 * LogEvent("Entered Mode Type", "Multiplayer", "6 Players");
	 * LogEvent("Entered Mode Type", "Multiplayer", "4 Players");
	 * LogEvent("Entered Mode Type", "Single Player");
	 * LogEvent("Final Score", "Multiplayer", 1234);
	 * LogEvent("Pressed Vehicle Selection Atleast Once");
	 * Remember to use firebase built in analytics strings when logging certain actions such as FirebaseAnalytics.EventLogin
	 */
	public static void LogEvent(string eventName)
	{
		#if analytics
			if(!IsReserved(eventName)){
				FirebaseAnalytics.LogEvent (eventName);

				ProjectManager.Log("[Analytics Event] " + eventName);
			}
		#endif
	}

	public static void LogEvent(string eventCategory, string eventValue)
	{
		#if analytics
			if(!IsReserved(eventCategory)){
				FirebaseAnalytics.LogEvent (eventCategory, "", eventValue);

				ProjectManager.Log("[Analytics Event] " + eventCategory + ", " + eventValue);
			}
		#endif
	}

	public static void LogEvent(string eventCategory, string eventLabel, string eventValue)
	{
		#if analytics
			if(!IsReserved(eventCategory)){
				FirebaseAnalytics.LogEvent (eventCategory, eventLabel, eventValue);

				ProjectManager.Log("[Analytics Event] " + eventCategory + ", " + eventLabel + ", " + eventValue);
			}
		#endif
	}

	public static void LogEvent(string eventCategory, string eventLabel, int eventValue)
	{
		#if analytics
			if(!IsReserved(eventCategory)){
				FirebaseAnalytics.LogEvent (eventCategory, eventLabel, eventValue);

				ProjectManager.Log("[Analytics Event] " + eventCategory + ", " + eventLabel + ", " + eventValue.ToString());
			}
		#endif
	}

	public static void LogEvent(string eventCategory, string eventLabel, float eventValue)
	{
		#if analytics
			if(!IsReserved(eventCategory)){
				FirebaseAnalytics.LogEvent (eventCategory, eventLabel, eventValue);

				ProjectManager.Log("[Analytics Event] " + eventCategory + ", " + eventLabel + ", " + eventValue.ToString());
			}
		#endif
	}

	public static void LogEvent(string eventCategory, string eventLabel, long eventValue)
	{
		#if analytics
			if(!IsReserved(eventCategory)){
				FirebaseAnalytics.LogEvent (eventCategory, eventLabel, eventValue);

				ProjectManager.Log("[Analytics Event] " + eventCategory + ", " + eventLabel + ", " + eventValue.ToString());
			}
		#endif
	}

	public static void LogScreen(string screenName)
	{
		#if analytics
			FirebaseAnalytics.LogEvent ("screen_view", "", screenName);

			ProjectManager.Log("[Analytics ScreenView] " + screenName);
		#endif
	}

	public static void LogError(string errorName, string errorDesc)
	{
		#if analytics
			Crashlytics.RecordCustomException(errorName, errorDesc, "");
			//FirebaseAnalytics.LogEvent (FirebaseAnalytics., errorName, isFatal ? "fatal" : "not fatal");

			ProjectManager.Log("[Analytics Error] " + errorName + ", " + errorDesc);
		#endif
	}

	// Forces the app to crash for crashlytics debugging purposes
	public static void ForceCrash()
	{
		#if analytics
			// Only allow this function to be called whilst the app is in debug mode
			if(ProjectManager.IsDebugModeActive()){
				ProjectManager.Log("[Analytics Debug] Forced a crash!");

				Crashlytics.Crash ();
			}
		#endif
	}

	// Force throw an none fatal exception for crashlytics debugging purposes
	public static void ForceException()
	{
		#if analytics
			// Only allow this function to be called whilst the app is in debug mode
			if(ProjectManager.IsDebugModeActive()){
				ProjectManager.Log("[Analytics Debug] Forced an exception!");

				Crashlytics.ThrowNonFatal ();
			}
		#endif
	}

	/// <summary>
	/// Sets the property for this user which is logged along with the events of this user.
	/// Warning: It is against the rules to log personal information even if it's hashed such as email or names!
	/// </summary>
	/// <param name="propertyName">Property name.</param>
	/// <param name="propertyValue">Property value.</param>
	public static void SetUserProperty(string propertyName, string propertyValue)
	{
		#if analytics
			if(!IsReserved(propertyName)){
				bool doesPropertyValueNeedLimiting = propertyValue.Length > 36;

				if(doesPropertyValueNeedLimiting)
					propertyValue = propertyValue.Substring(0, 36);

				FirebaseAnalytics.SetUserProperty (propertyName, propertyValue);
				Crashlytics.SetKeyValue(propertyName, propertyValue);

				ProjectManager.Log("[Analytics User Property] " + propertyName + " = " + propertyValue);
			}
		#endif
	}

	private static bool IsReserved (string inPropertyName)
	{
		// Only check the reserved string in debug mode or the editor because string comparison isn't cheap for all analytics calls
		if (Application.isEditor || ProjectManager.IsDebugModeActive ()) {
			string[] reservedEventNames = {
				"app_clear_data",
				"app_uninstall",
				"app_update",
				"error",
				"first_open",
				"first_visit",
				"in_app_purchase",
				"notification_dismiss",
				"notification_foreground",
				"notification_open",
				"notification_receive",
				"os_update",
				"session_start",
				"user_engagement"
			};

			foreach (string eventName in reservedEventNames)
				if (inPropertyName.Contains (eventName)) {
					ProjectManager.Log("[Analytics Log Failure] " + inPropertyName + " is reserved and cannot be used as an event name!");

					return true;
				}
		}

		return false;
	}
}
