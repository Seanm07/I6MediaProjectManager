using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class I6MediaPlugin : MonoBehaviour {

	#if UNITY_ANDROID && !UNITY_EDITOR && ias
		private static AndroidJavaObject activityContext;
		private static AndroidJavaClass javaClass;

		private static bool isSetup = false;
	#endif

	void Awake()
	{
		ClassSetup();
	}

	void Start()
	{
		#if UNITY_ANDROID && !UNITY_EDITOR && ias
			if(!isSetup)
				Analytics.LogError("I6MediaPlugin", "Failed to attach current thread to Java VM");
		#endif
	}

	private static void ClassSetup()
	{
		#if UNITY_ANDROID && !UNITY_EDITOR && ias
			if(!isSetup && (activityContext == null || javaClass == null)){
				if(AndroidJNI.AttachCurrentThread() >= 0){
					activityContext = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
					javaClass = new AndroidJavaClass("com.pickle.PackageLister.PackageLister");
				} else {
					isSetup = true;
				}
			}
		#endif
	}

	/// <summary>
	/// Make the device vibrate a bit to give the user a response to their touch or to notify them
	/// </summary>
	/// <param name="Type">Haptic vibration type (1 = quick & weak, 2 = quick & mid, 3 = longer & strong)</param>
	public static void DoHapticFeedback(int Type = 1)
	{
		#if UNITY_ANDROID && !UNITY_EDITOR && ias
			ClassSetup();

			if(javaClass != null && activityContext != null){
				javaClass.CallStatic("HapticFeedback", activityContext, Type);
			} else {
				Analytics.LogError("I6MediaPlugin", "Haptic feedback failed!");
			}
		#endif
	}

	public static long GetAppInstallTimestamp()
	{
		#if UNITY_ANDROID && !UNITY_EDITOR && ias
			ClassSetup();

			if(javaClass != null && activityContext != null){
				return javaClass.CallStatic<long>("GetInstallTimestamp", activityContext);
			} else {
				Analytics.LogError("I6MediaPlugin", "Get install timestamp failed!");
			}
		#endif

		// Return -1L to show it failed
		return -1L;
	}

	public static int GetDensity()
	{
		#if UNITY_ANDROID && !UNITY_EDITOR && ias
			ClassSetup();

			if(javaClass != null && activityContext != null){
				return javaClass.CallStatic<int>("GetDensity", activityContext);
			} else {
				Analytics.LogError("I6MediaPlugin", "Get density failed!");
			}
		#endif

		// Nothing has been returned yet so just return Screen.dpi instead (Note that this will return 0 if it fails)
		return Mathf.RoundToInt(Screen.dpi);
	}

	public static string GetSelfPackageName()
	{
		string PackageName = "Unknown";

		#if UNITY_ANDROID && !UNITY_EDITOR && ias
			ClassSetup();

			if(javaClass != null && activityContext != null){
				// Get the package name being used by this app on the current device
				PackageName = javaClass.CallStatic<string>("GetSelfPackageName", activityContext);
			} else {
				Analytics.LogError("I6MediaPlugin", "Get self package name failed!");
			}
		#endif

		return PackageName;
	}

	public static string GetPackageList(string searchString = default(string))
	{
		#if UNITY_ANDROID && !UNITY_EDITOR && ias
			ClassSetup();			

			string PackageList = string.Empty;

			if(javaClass != null && activityContext != null){
				// Get the list of installed packages on the device
				PackageList = javaClass.CallStatic<string>("GetPackageList", activityContext, searchString);
			} else {
				Analytics.LogError("I6MediaPlugin", "Get package list failed!");
			}

			return PackageList;
		#endif

		return string.Empty;
	}

	public static void DisplayToastMessage(string inString)
	{
		#if UNITY_ANDROID && !UNITY_EDITOR && ias
			ClassSetup();

			if(javaClass != null){
				// Get the list of installed packages on the device
				javaClass.CallStatic("DisplayToast", activityContext, inString , 5);
			} else {
				Analytics.LogError("I6MediaPlugin", "Display toast message failed!");
			}
		#endif
	}

	public static void CancelToastMessage()
	{
		#if UNITY_ANDROID && !UNITY_EDITOR && ias
			ClassSetup();

			if(javaClass != null){
				// Get the list of installed packages on the device
				javaClass.CallStatic("ForceEndToast");
			} else {
				Analytics.LogError("I6MediaPlugin", "Cancel toast message failed!");
			}
		#endif
	}
}
