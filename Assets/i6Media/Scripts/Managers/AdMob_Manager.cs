/*
 * Last updated 2nd June 2017
 * Written by Sean McManus for i6 Media sean@i6.com
 * 
 * This script wasn't fully rewritten with the plugin changeover so developers should be able to implement it with minimal changes
 * Eventually this script will be cleaned up a bit and wrapped with #if admob defines but for now it's just being left as it is
 */


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;

using GoogleMobileAds;
using GoogleMobileAds.Api;

using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ActionStateChecks
{
	public bool State { get; set; }
	public string LogMessage { get; private set; }

	public ActionStateChecks(bool inState, string inLogMessage)
	{
		State = inState;
		LogMessage = inLogMessage;
	}
}

public class ActionStateItems
{
	public List<ActionStateChecks> items = new List<ActionStateChecks>();

	public void UpdateItems(List<bool> inItems)
	{
		if(items.Count != inItems.Count)
			Debug.Log ("Invalid inItem count!");

		for(int i=0;i < items.Count;i++)
			items[i].State = inItems[i];
	}
}

#if UNITY_EDITOR
public class ReadOnlyAttribute : PropertyAttribute{}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUI.GetPropertyHeight(property, label, true);
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		GUI.enabled = false;
		EditorGUI.PropertyField(position, property, label, true);
		GUI.enabled = true;
	}
}
#endif

public class AdMob_Manager : MonoBehaviour
{
	public static AdMob_Manager Instance;
	public static bool AdMobAndroidReady = false;

	[Header("AdMob Script Settings")]
	public bool EnableAdMob = true; // Is admob enabled? Will any banners or interstitials be triggered?
	public bool DebugLogging = false; // Should debug messages be logged?

	// TagForChildDirectedTreatment will stop admob tracking this user and will not deliver interest based ads
	// This will reduce revenue for the game so don't set this unless specifically asked to!
	#if UNITY_EDITOR 
	[ReadOnly] 
	#endif
	public bool TagForChildDirectedTreatment = false;

	[FormerlySerializedAs("interstitialID")] public string InterstitialId = "ca-app-pub-xxxxxxxxxxxxxxxx/xxxxxxxxxx"; // AdMob interstitial ID (You will be given a unique ID per project!)
	[FormerlySerializedAs("bannerID")] public string BannerId = "ca-app-pub-xxxxxxxxxxxxxxxx/xxxxxxxxxx"; // AdMob banner ID (You will be given a unique ID per project!)

	public enum BannerAdTypeList { Default, NativeExpressAds };
	public BannerAdTypeList BannerAdType = BannerAdTypeList.Default;

	[Header("Test Mode Settings")]
	public bool EnableTestMode = false; // Test mode will display test ads which we are allowed to click
	public List<string> TestDeviceIds = new List<string>(); // List of device ids which will be in test mode when test mode is enabled

	[Header("Advert Control Settings")]
	public bool IntLoadOnStart = false; // Should an interstitial be LOADED (Not shown) when this script starts?
	public bool IntAutoReload = false; // Shown an interstitial be LOADED (Not shown) as soon as a previous interstitial is closed (always keeping an interstitial ready in memory)
	public bool BannerLoadOnStart = false; // Should a banner be LOADED (Not shown) when this script starts?
	public bool BannerShowOnStart = false; // Should a banner be SHOWN when this script starts?
	public AdPosition BannerLoadOnStartPos; // Position of banner which will be loaded on start

	// CHANGING THIS DOES NOTHING! This is just to show the developer that banners on start will be smart banners (see line below for why we can't adsize in inspector)
	public enum BannerLoadOnStartSizeList { SmartBanner }; // Dummy variable
	#if UNITY_EDITOR 
	[ReadOnly] 
	#endif
	public BannerLoadOnStartSizeList BannerLoadOnStartSize; // Dummy variable
	//public AdSize BannerLoadOnStartSize; // Banner type which will be loaded on start (Not currently exposed to inspector in Google AdMob plugin)

	[Header("Pre-Interstitial Settings")]
	public bool UseInterstitialWaitScreen = true;
	public GameObject InterstitialWaitScreen; // Screen to show before an interstitial pops
	public float InterstitialWaitTime = 1f; // Time to wait before displaying interstitial after InterstitialWaitScreen has appeared

	// Information about the interstitial state
	public bool IntIsReady { get; private set; }
	public bool IntIsLoading { get; private set; }
	public bool IntIsVisible { get; private set; }
	public bool IntWantedVisible { get; private set; }

	// Information about the banner state
	public bool BannerIsReady { get; private set; }
	public bool BannerIsLoading { get; private set; }
	public bool BannerIsVisible { get; private set; }
	public bool BannerWantedVisible { get; private set; }

	// Cache the type of the current banner in memory so we can process calls to LoadBanner again to change certain things without needing to actually request another banner
	public AdSize BannerInMemoryType { get; set; }
	public AdPosition BannerInMemoryLayout { get; set; }

	// If we're hiding a banner due to an overlay (popup box or backscreen) then we want to remember the ad state when that is closed
	public bool BannerPrevState { get; private set; }

	// Sometimes we like to overlay our overlays but still want to remember our original banner state
	public int BannerOverlayDepth { get; set; }

	public Dictionary<string, ActionStateItems> ActionState = new Dictionary<string, ActionStateItems>();

	public BannerView AdMobBanner { get; private set; }
	public NativeExpressAdView AdMobNativeBanner { get; private set; }
	public InterstitialAd AdMobInterstitial { get; private set; }

	void Awake()
	{
		if(Instance){
			DebugLog("You have duplicate AdMob_Manager.cs scripts in your scene! Admob might not work as expected!");
			return;
		}

		Instance = (Instance == null ? this : Instance);

		SetupRequirements ();

		if(!EnableAdMob){
			DebugLog("AdMob is NOT enabled! No adverts will be triggered!", true);
			return;
		}

		if(EnableTestMode) DebugLog("This build has admob set to debug mode! Remember to disable before release!", true);

		if (TagForChildDirectedTreatment) DebugLog ("Ads for this game are being tagged for child directed treatment!", true);

		// LOAD (not display) an interstitial on awake if the option is selected
		if(IntLoadOnStart) LoadInterstitial(false);

		// LOAD (not display) a banner if the option is selected
		if(BannerLoadOnStart) LoadBanner(AdSize.SmartBanner, BannerLoadOnStartPos, BannerShowOnStart);
	}

	private void DebugLog(string Message, bool IgnoreDebugSetting = false)
	{
		if(!DebugLogging && !IgnoreDebugSetting)
			return;

		// Prepend AdMobManager to the debug output to make it easier to filter in logcat
		Debug.Log ("AdMobManager " + Message);
	}

	// Setup requirements for the different functions and what they log if not met
	private void SetupRequirements()
	{
		ActionState.Add("LoadInterstitial", new ActionStateItems());
		ActionState["LoadInterstitial"].items.Add(new ActionStateChecks(IntIsLoading, "Already loading!"));	// Don't load interstitial if one is already loading
		ActionState["LoadInterstitial"].items.Add(new ActionStateChecks(IntIsReady, "Already ready!"));		// Don't load interstitial if one is already loaded
		ActionState["LoadInterstitial"].items.Add(new ActionStateChecks(IntIsVisible, "Already visible!"));	// Don't load interstitial if one is already visible

		ActionState.Add("LoadBanner", new ActionStateItems());
		ActionState["LoadBanner"].items.Add(new ActionStateChecks(BannerIsLoading, "Already loading!"));	// Don't load banner if one is already loading
		ActionState["LoadBanner"].items.Add(new ActionStateChecks(BannerIsReady, "Already ready!"));		// Don't load banner if one is already loaded
		ActionState["LoadBanner"].items.Add(new ActionStateChecks(BannerIsVisible, "Already visible!"));	// Don't load banner if one is already visible

		ActionState.Add("RepositionBanner", new ActionStateItems());
		ActionState["RepositionBanner"].items.Add(new ActionStateChecks(BannerIsReady, "Not ready!"));		// Don't reposition banner if it's not yet ready

		ActionState.Add("ShowInterstitial", new ActionStateItems());
		ActionState["ShowInterstitial"].items.Add(new ActionStateChecks(IntIsVisible, "Already visible!"));	// Don't show interstitial if one is already visible

		ActionState.Add("ShowBanner", new ActionStateItems());
		ActionState["ShowBanner"].items.Add(new ActionStateChecks(BannerIsVisible, "Already visible!"));	// Don't show banner if one is already visible
	}

	private bool CanPerformAction(string ActionName, List<ActionStateChecks> ActionChecks)
	{
		// Iterate through the checks in the current item
		for(int i=0;i < ActionChecks.Count;i++)
		{
			// Return false and log the event if any of the check states are false
			if(!ActionChecks[i].State){
				//Analytics.LogEvent("AdMob", ActionName + " failed", ActionChecks[i].LogMessage);
				DebugLog(ActionName + " failed - " + ActionChecks[i].LogMessage);
				return false;
			}
		}

		return true;
	}

	private AdRequest GenerateAdRequest()
	{
		AdRequest.Builder AdBuilder = new AdRequest.Builder();

		AdBuilder.AddTestDevice(AdRequest.TestDeviceSimulator); // Marks emulators as testers

		// Add all of our TestDeviceIds as test devices if we're in test mode
		if (EnableTestMode) {
			foreach (string CurDeviceId in TestDeviceIds) {
				AdBuilder.AddTestDevice (CurDeviceId);
			}
		}
		AdBuilder.TagForChildDirectedTreatment(TagForChildDirectedTreatment);

		return AdBuilder.Build();
	}

	/// <summary>
	/// Loads an interstitial advert into memory.
	/// </summary>
	/// <param name="DisplayImmediately">If set to <c>true</c> display the interstitial immediately when it has finished loading.</param>
	public void LoadInterstitial(bool DisplayImmediately = false, bool UseWaitScreen = false)
	{
		if(!EnableAdMob) return;

		// Get the name of the current method
		string MethodName = "LoadInterstitial";

		DebugLog(MethodName + " called - DisplayImmediately: " + DisplayImmediately);

		// Update the state items (Values used to determine if the action in this method should be ran)
		ActionState[MethodName].UpdateItems(new List<bool>(){ !IntIsLoading, !IntIsReady, !IntIsVisible });

		// Check if we can perform the action for the current method
		if(CanPerformAction(MethodName, ActionState[MethodName].items)){
			// Mark the interstitial as loading
			IntIsLoading = true;

			// If we want to display the interstitial as soon as it's loaded then mark the wanted visible variable as true
			IntWantedVisible = DisplayImmediately;

			// Load an interstitial ad marking it as hidden, this script will handle showing the interstitial
			AdMobInterstitial = new InterstitialAd(InterstitialId);

			// Register the interstitial ad events
			AdMobInterstitial.OnAdLoaded += OnReceiveInterstitial;
			AdMobInterstitial.OnAdFailedToLoad += OnFailedReceiveInterstitial;
			AdMobInterstitial.OnAdOpening += OnInterstitialVisible;
			AdMobInterstitial.OnAdClosed += OnInterstitialHidden;
			AdMobInterstitial.OnAdLeavingApplication += OnInterstitialClick;	

			AdMobInterstitial.LoadAd(GenerateAdRequest());
		} else {
			if(DisplayImmediately)
				ShowInterstitial(UseWaitScreen);
		}
	}

	private void LoadInterstitial(bool DisplayImmediately, bool UseWaitScreen, bool ForcedInternalCall)
	{
		if(ForcedInternalCall){
			LoadInterstitial(false, UseWaitScreen);
			IntWantedVisible = DisplayImmediately;
		} else {
			LoadInterstitial(DisplayImmediately, UseWaitScreen);
		}
	}

	/// <summary>
	/// Shows an interstitial if one is loaded in memory.
	/// </summary>
	/// <param name="UseWaitScreen">The wait screen will enable the InterstitialWaitScreen prefab and wait InterstitialWaitTime seconds before showing the interstitial</param>
	public void ShowInterstitial(bool UseWaitScreen = false)
	{
		if(!EnableAdMob) return;

		// Get the name of the current method
		string MethodName = "ShowInterstitial";

		DebugLog(MethodName + " called");

		// Update the state items (Values used to determine if the action in this method should be ran)
		ActionState[MethodName].UpdateItems(new List<bool>(){ !IntIsVisible });

		// Check if we can perform the action for the current method
		if(CanPerformAction(MethodName, ActionState[MethodName].items)){
			if(IntIsReady){
				if(UseWaitScreen && UseInterstitialWaitScreen){
					// We're ready to show the interstitial but first a message from our sponsors err I mean a black screen wait wait text on it
					if(InterstitialWaitScreen != null){						
						StartCoroutine(ShowInterstitialAfterDelay());
					} else {
						DebugLog("Wait screen was enabled but no gameobject was set! Interstitial will not be delayed..", true);

						// Show the interstitial
						AdMobInterstitial.Show();
					}
				} else {
					// Show the interstitial
					AdMobInterstitial.Show();
				}
			} else {
				LoadInterstitial(true, UseWaitScreen, true);
			}
		}
	}

	#if UNITY_4
	// Unity 4 doesn't support WaitForSecondsRealtime so we have to do a hacky timescale switch back to 1f whilst we run WaitForSeconds instead
	private float TimescalePrePause;

	private IEnumerator ShowInterstitialAfterDelay()
	{
	TimescalePrePause = Time.timeScale;

	Time.timeScale = 1f;

	// Enable the wait screen
	InterstitialWaitScreen.SetActive(true);

	yield return new WaitForSeconds (InterstitialWaitTime);

	// Show the interstitial
	AdMobInterstitial.Show();

	float WaitTime = 0f;

	// Wait for IntIsReady to become false OR 3 seconds to pass
	while (IntIsReady && WaitTime < 3f) {
	WaitTime += Time.unscaledDeltaTime;
	yield return null;
	}

	CancelInterstitial(); // If an interstitial still hasn't popped cancel the showing of it

	// If an ad didn't pop earlier then wait half a second to make sure no ads will be displayed past this point
	if(IntIsReady) yield return new WaitForSeconds(0.5f);

	// Disable the wait screen
	InterstitialWaitScreen.SetActive(false);

	// Unpause the game
	Time.timeScale = TimescalePrePause;
	}
	#elif UNITY_5
	private IEnumerator ShowInterstitialAfterDelay()
	{
		// Enable the wait screen
		InterstitialWaitScreen.SetActive(true);

		yield return new WaitForSecondsRealtime (InterstitialWaitTime);

		AdMobInterstitial.Show ();

		float WaitTime = 0f;

		// Wait for IntIsReady to become false OR 3 seconds to pass
		while (IntIsReady && WaitTime < 3f) {
			WaitTime += Time.unscaledDeltaTime;
			yield return null;
		}

		CancelInterstitial (); // If an interstitial still hasn't popped cancel the showing of it

		// If an ad didn't pop earlier then wait half a second to make sure no ads will be displayed past this point
		if(IntIsReady) yield return new WaitForSecondsRealtime(0.5f);

		// Disable the wait screen
		InterstitialWaitScreen.SetActive(false);
	}
	#endif

	/// <summary>
	/// Cancels an interstitial from loading, useful if you wanted to show an interstitial on menu x but it didn't load in time, 
	/// you might want to cancel the interstitial from showing once the player enters the main game for example.
	/// </summary>
	public void CancelInterstitial()
	{
		if(!EnableAdMob) return;

		// Mark the interstitial as not wanted to show
		IntWantedVisible = false;

		DebugLog("Cancelling interstitial!");
	}

	/// <summary>
	/// Clears an interstitial from memory and sets all interstitial pending values to false.
	/// </summary>
	public void DestroyInterstitial()
	{
		if(!EnableAdMob) return;

		IntWantedVisible = false;
		IntIsReady = false;
		IntIsVisible = false;
		IntIsLoading = false;

		AdMobInterstitial.Destroy();

		DebugLog("Destroying interstitial!");
	}

	/// <summary>
	/// Loads a banner advert into memory.
	/// </summary>
	/// <param name="Width">Width of the admob banner</param>
	/// <param name="Height">Height of the admob banner</param>
	/// <param name="AdLayout">Admob ad position</param>
	/// <param name="DisplayImmediately">If set to <c>true</c> display immediately when it has finished loading.</param>
	public void LoadBanner(int Width, int Height, AdPosition AdLayout, bool DisplayImmediately = false)
	{
		LoadBanner (new AdSize (Width, Height), AdLayout, DisplayImmediately);
	}

	/// <summary>
	/// Loads a banner advert into memory.
	/// </summary>
	/// <param name="AdType">Admob banner ad type.</param>
	/// <param name="AdLayout">Admob ad position.</param>
	/// <param name="DisplayImmediately">If set to <c>true</c> display the banner immediately when it has finished loading.</param>
	public void LoadBanner(AdSize AdType, AdPosition AdLayout, bool DisplayImmediately = false)
	{
		if(!EnableAdMob) return;

		// Get the name of the current method
		string MethodName = "LoadBanner";

		DebugLog(MethodName + " called for " + AdType + " - DisplayImmediately: " + DisplayImmediately);

		// Update the state items (Values used to determine if the action in this method should be ran)
		ActionState[MethodName].UpdateItems(new List<bool>(){ !BannerIsLoading, !BannerIsReady, !BannerIsVisible });

		// Check if we can perform the action for the current method
		if(CanPerformAction(MethodName, ActionState[MethodName].items)){
			// Mark the banner as loading
			BannerIsLoading = true;

			// If we want to display the banner as soon as it's loaded then mark the wanted visible variable as true
			BannerWantedVisible = DisplayImmediately;

			switch (BannerAdType) {
				case BannerAdTypeList.Default:
					// Load a banner ad marking it as hidden, this script will handle showing the banner
					AdMobBanner = new BannerView (BannerId, AdType, AdLayout);

					// Register the banner ad events
					AdMobBanner.OnAdLoaded += OnReceiveBanner;
					AdMobBanner.OnAdFailedToLoad += OnFailReceiveBanner;
					AdMobBanner.OnAdOpening += OnBannerVisible;
					AdMobBanner.OnAdClosed += OnBannerHidden;
					AdMobBanner.OnAdLeavingApplication += OnBannerClick;

					AdMobBanner.LoadAd (GenerateAdRequest ());
					break;

				case BannerAdTypeList.NativeExpressAds:
					// Load a native banner ad marking it as hidden, this script will handle showing the banner
					AdMobNativeBanner = new NativeExpressAdView (BannerId, AdType, AdLayout);

					// Register the native banner ad events
					AdMobNativeBanner.OnAdLoaded += OnReceiveBanner;
					AdMobNativeBanner.OnAdFailedToLoad += OnFailReceiveBanner;
					AdMobNativeBanner.OnAdOpening += OnBannerVisible;
					AdMobNativeBanner.OnAdClosed += OnBannerHidden;
					AdMobNativeBanner.OnAdLeavingApplication += OnBannerClick;

					AdMobNativeBanner.LoadAd (GenerateAdRequest ());
					break;
			}

			BannerInMemoryType = AdType;
			BannerInMemoryLayout = AdLayout;
		} else {
			// If this was just a call to set the banner to the same type and position as it is already in then just ignore it
			if (AdType != BannerInMemoryType || AdLayout != BannerInMemoryLayout) {
				// Google Mobile Ads does not currently support banner repositioning so we are forced to treat new positions as changing to a new banner type (destroying and reloading the advert)
				DestroyBanner ();
				LoadBanner (AdType, AdLayout, DisplayImmediately);
			} else {
				// This is an ad we already have loaded in memory, if display immediately is true then force show the ad
				if (DisplayImmediately) {
					ShowBanner ();
				}
			}
		}
	}

	/// <summary>
	/// Loads a banner advert into memory.
	/// </summary>
	/// <param name="Width">Width of the admob banner</param>
	/// <param name="Height">Height of the admob banner</param>
	/// <param name="XPos">X placement position relative to top left</param>
	/// <param name="YPos">Y placement position relative to top left</param>
	/// <param name="DisplayImmediately">If set to <c>true</c> display immediately when it has finished loading.</param>
	public void LoadBanner(int Width, int Height, int XPos, int YPos, bool DisplayImmediately = false)
	{
		LoadBanner (new AdSize (Width, Height), XPos, YPos, DisplayImmediately);
	}

	/// <summary>
	/// Loads a banner advert into memory.
	/// </summary>
	/// <param name="AdType">Admob banner ad type.</param>
	/// <param name="XPos">X placement position relative to top left</param>
	/// <param name="YPos">Y placement position relative to top left</param>
	/// <param name="DisplayImmediately">If set to <c>true</c> display immediately when it has finished loading.</param>
	public void LoadBanner(AdSize AdType, int XPos, int YPos, bool DisplayImmediately = false)
	{
		if(!EnableAdMob) return;

		// Get the name of the current method
		string MethodName = "LoadBanner";

		DebugLog(MethodName + " called for " + AdType + " - DisplayImmediately: " + DisplayImmediately);

		// Update the state items (Values used to determine if the action in this method should be ran)
		ActionState[MethodName].UpdateItems(new List<bool>(){ !BannerIsLoading, !BannerIsReady, !BannerIsVisible });

		// Check if we can perform the action for the current method
		if(CanPerformAction(MethodName, ActionState[MethodName].items)){
			// Mark the banner as loading
			BannerIsLoading = true;

			// If we want to display the banner as soon as it's loaded then mark the wanted visible variable as true
			BannerWantedVisible = DisplayImmediately;

			switch (BannerAdType) {
				case BannerAdTypeList.Default:
					// Load a banner ad marking it as hidden, this script will handle showing the banner
					AdMobBanner = new BannerView (BannerId, AdType, XPos, YPos);

					// Register the banner ad events
					AdMobBanner.OnAdLoaded += OnReceiveBanner;
					AdMobBanner.OnAdFailedToLoad += OnFailReceiveBanner;
					AdMobBanner.OnAdOpening += OnBannerVisible;
					AdMobBanner.OnAdClosed += OnBannerHidden;
					AdMobBanner.OnAdLeavingApplication += OnBannerClick;

					AdMobBanner.LoadAd (GenerateAdRequest ());
					break;

				case BannerAdTypeList.NativeExpressAds:
					// Load a native banner ad marking it as hidden, this script will handle showing the banner
					AdMobNativeBanner = new NativeExpressAdView (BannerId, AdType, XPos, YPos);

					// Register the native banner ad events
					AdMobNativeBanner.OnAdLoaded += OnReceiveBanner;
					AdMobNativeBanner.OnAdFailedToLoad += OnFailReceiveBanner;
					AdMobNativeBanner.OnAdOpening += OnBannerVisible;
					AdMobNativeBanner.OnAdClosed += OnBannerHidden;
					AdMobNativeBanner.OnAdLeavingApplication += OnBannerClick;

					AdMobNativeBanner.LoadAd (GenerateAdRequest ());
					break;
			}

			BannerInMemoryType = AdType;
			BannerInMemoryLayout = AdPosition.TopLeft;
		} else {
			// Google Mobile Ads does not currently support banner repositioning so we are forced to treat new positions as changing to a new banner type (destroying and reloading the advert)
			DestroyBanner ();
			LoadBanner (AdType, XPos, YPos, DisplayImmediately);
		}
	}

	private void LoadBanner(bool DisplayImmediately, bool ForcedInternalCall)
	{
		if(ForcedInternalCall){
			LoadBanner(BannerInMemoryType, BannerInMemoryLayout, false);
			BannerWantedVisible = DisplayImmediately;
		} else {
			LoadBanner(BannerInMemoryType, BannerInMemoryLayout, DisplayImmediately);
		}
	}

	/// <summary>
	/// Shows a banner advert if one is loaded in memory.
	/// </summary>
	public void ShowBanner(bool ForceShow = false)
	{
		if(!EnableAdMob) return;

		// Get the name of the current method
		string MethodName = "ShowBanner";

		DebugLog(MethodName + " called");

		// Check if we're calling ShowBanner because we're returning from an overlay screen which hid the banner
		if(BannerOverlayDepth > 0 && !ForceShow){
			// Decrease the overlay depth by 1
			BannerOverlayDepth--;

			// If the overlay depth is still above 0 then there must still be some overlays open
			if(BannerOverlayDepth > 0)
				return;

			// There isn't any more overlaying menus open, return to the previous banner ad state
			BannerWantedVisible = BannerPrevState;

			DebugLog ("Banner wanted set to prev state: " + BannerPrevState);
		} else {
			BannerWantedVisible = true;
		}

		if(!BannerWantedVisible) return;

		// Update the state items (Values used to determine if the action in this method should be ran)
		ActionState[MethodName].UpdateItems(new List<bool>(){ !BannerIsVisible });

		// Check if we can perform the action for the current method
		if (CanPerformAction (MethodName, ActionState [MethodName].items)) {
			if (BannerIsReady) {
				// Show the banner
				switch (BannerAdType) {
					case BannerAdTypeList.Default:
						AdMobBanner.Show ();
						break;

					case BannerAdTypeList.NativeExpressAds:
						AdMobNativeBanner.Show ();
						break;
				}
			} else {
				LoadBanner (true, true);
			}
		}
	}

	/// <summary>
	/// Hides a banner advert, will also cancel a banner advert from showing if one is loaded.
	/// </summary>
	/// <param name="IsOverlay">Set to <c>true</c> if you want to hide the banner while opening an overlaying screen (such as the backscreen) and want to revert the banner ad status later.</param>
	public void HideBanner(bool IsOverlay = false)
	{
		if(!EnableAdMob) return;

		// If this is an overlaying screen (e.g backscreen) then we'll want to return to the previous banner state when we close it
		if(IsOverlay){
			BannerOverlayDepth++;

			if(BannerOverlayDepth == 1)
				BannerPrevState = BannerWantedVisible;
		}

		DebugLog("Hiding banner!");

		// Mark wanted visible as false so if the banner ad hasn't loaded yet it'll make sure it isn't shown when loaded
		BannerWantedVisible = false;
		BannerIsVisible = false;

		// Hide the banner advert from view (This does not unload it from memory)
		switch (BannerAdType) {
			case BannerAdTypeList.Default:
				if(AdMobBanner != default(BannerView))
					AdMobBanner.Hide();
				break;

			case BannerAdTypeList.NativeExpressAds:
				if(AdMobNativeBanner != default(NativeExpressAdView))
					AdMobNativeBanner.Hide();
				break;
		}

	}

	/// <summary>
	/// Remove the banner from memory. (Required if you want to load a new banner ad type, however it's automatic when calling to load a new banner)
	/// </summary>
	public void DestroyBanner()
	{
		if(!EnableAdMob) return;

		// Mark the banner properties as false as the banner is now destroyed
		BannerWantedVisible = false;
		BannerIsLoading = false;
		BannerIsReady = false;
		BannerIsVisible = false;
		BannerInMemoryType = default(AdSize);
		BannerInMemoryLayout = default(AdPosition);

		DebugLog("Destroying banner!");

		// Unload the banner from memory
		switch (BannerAdType) {
			case BannerAdTypeList.Default:
				AdMobBanner.Destroy();
				break;

			case BannerAdTypeList.NativeExpressAds:
				AdMobNativeBanner.Destroy ();
				break;
		}
	}

	// Everything past this point is ad listener events //

	#region Banner callback handlers

	public void OnReceiveBanner(object Sender, EventArgs Args)
	{
		BannerIsReady = true;
		BannerIsLoading = false;

		DebugLog ("New banner loaded!");

		if(BannerWantedVisible){
			ShowBanner();
		} else {
			HideBanner();
		}
	}

	public void OnFailReceiveBanner(object Sender, AdFailedToLoadEventArgs Args)
	{
		BannerIsReady = false;

		DebugLog("Banner failed to load - Error: " + Args.Message);
		Analytics.LogEvent ("AdMob", "Banner Load Failure", "Error: " + Args.Message);

		if(Application.internetReachability != NetworkReachability.NotReachable)
			StartCoroutine(RetryBannerLoad());
	}

	private IEnumerator RetryBannerLoad()
	{
		yield return new WaitForSeconds(2f);

		DebugLog("Retrying banner load!");

		if(!BannerIsLoading){
			BannerInMemoryType = default(AdSize);
			BannerInMemoryLayout = default(AdPosition);

			LoadBanner(BannerInMemoryType, BannerInMemoryLayout, BannerWantedVisible);
		}
	}

	public void OnBannerVisible(object Sender, EventArgs Args)
	{
		BannerIsVisible = true;

		if (!BannerWantedVisible) {
			HideBanner ();
		}
	}

	public void OnBannerHidden(object Sender, EventArgs Args)
	{
		BannerIsVisible = false;

		if(BannerWantedVisible)
			ShowBanner();
	}

	public void OnBannerClick(object Sender, EventArgs Args)
	{
		Analytics.LogEvent("AdMob", "Banner Clicked");
	}

	#endregion

	#region Interstitial callback handlers

	public void OnReceiveInterstitial(object Sender, EventArgs Args)
	{
		IntIsReady = true;
		IntIsLoading = false;

		if(IntWantedVisible)
			ShowInterstitial();
	}

	public void OnFailedReceiveInterstitial(object Sender, AdFailedToLoadEventArgs Args)
	{
		IntIsReady = false;
		IntIsLoading = false;

		DebugLog("Interstitial failed to load - Error: " + Args.Message);
		Analytics.LogEvent("AdMob", "Interstitial Load Failure", "Error: " + Args.Message);

		if(Application.internetReachability != NetworkReachability.NotReachable)
			StartCoroutine(RetryInterstitialLoad());
	}

	private IEnumerator RetryInterstitialLoad()
	{
		yield return new WaitForSeconds(2f);

		DebugLog("Retrying interstitial load!");

		if(!IntIsLoading) LoadInterstitial (IntWantedVisible);
	}

	public void OnInterstitialVisible(object Sender, EventArgs Args)
	{
		IntIsVisible = true;

		// The interstitial auto hides ads but lets call the usual functions so the variables know what's happening
		HideBanner(true);
	}

	public void OnInterstitialHidden(object Sender, EventArgs Args)
	{
		IntIsReady = false;
		IntIsVisible = false;

		if(IntAutoReload)
			LoadInterstitial(false);

		// The interstitial auto hid any banners, so we need to re-enable them
		ShowBanner();
	}

	public void OnInterstitialClick(object Sender, EventArgs Args)
	{
		Analytics.LogEvent("AdMob", "Interstitial Clicked");
	}

	#endregion

}