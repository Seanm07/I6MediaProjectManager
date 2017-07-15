﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

#if UNITY_EDITOR
	using UnityEditor;
#endif

// Storage classes for IAS adverts
[Serializable]
public class AdJsonFileData {
	public List<AdSlotData> slotInts = new List<AdSlotData>();

	public AdJsonFileData(List<AdSlotData> newSlotIntsData = null)
	{
		slotInts = (newSlotIntsData == null ? new List<AdSlotData>() : newSlotIntsData);
	}
}

[Serializable]
public class AdSlotData {
	public int slotInt; // Number from the slotID

	public List<AdData> advert = new List<AdData>();

	public int lastSlotId;

	public AdSlotData(int newSlotInt = -1, List<AdData> newAdvert = null)
	{
		slotInt = newSlotInt;
		advert = (newAdvert == null ? new List<AdData>() : newAdvert);
	}
}

[Serializable]
public class AdData {
	public char slotChar; // Character from the slotID
	public string fileName; // Name this ad file will be named as on the device

	public bool isTextureFileCached; // Has the texture been saved to the device
	public bool isTextureReady; // Has the texture finished downloading
	public bool isInstalled; // Is this an advert for an app which is already installed?
	public bool isSelf; // Is this an advert for this game?
	public bool isActive; // Is this an advert marked as active in the json file?

	public long lastUpdated; // Timestamp of when the ad was last updated
	public long newUpdateTime; // Timestamp of the newly collected ad data

	public string imgUrl; // URL of the image we need to download
	public string adUrl; // URL the player is taken to when clicking the ad
	public string packageName;

	public int adTextureId = -1; // Reference id to which the texture for this ad is stored in

	public AdData(char inSlotChar = default(char))
	{
		slotChar = inSlotChar;
	}
}

// These classes are for the JsonUtility to move the data into after they've been ready from the file
[Serializable]
public class JsonFileData {
	public List<JsonSlotData> slots;
	public List<JsonSlotData> containers;
}

[Serializable]
public class JsonSlotData {
	public string slotid;

	public long updatetime;
	public bool active;

	public string adurl;
	public string imgurl;
}

public class IAS : MonoBehaviour
{
	public static IAS staticRef;

	#if !UNITY_5 || !UNITY_ANDROID
		public string bundleId = "com.example.GameNameHere";
		public string appVersion = "1.00";
	#else
		public string bundleId { get; private set; }
		public string appVersion { get; private set; }
	#endif

	private int internalScriptVersion = 13;

	// JSON URLs where the ads are grabbed from
	#if UNITY_IOS
		public string[] jsonUrls = new string[1]{"http://ias.gamepicklestudios.com/ad/3.json"};
	#elif UNITY_WP_8_1
		public string[] jsonUrls = new string[1]{"http://ias.gamepicklestudios.com/ad/4.json"};
	#else
		public string[] jsonUrls = new string[1]{"http://ias.gamepicklestudios.com/ad/1.json"};
	#endif

	private int slotIdDecimalOffset = 97; // Decimal offset used to start our ASCII character at 'a'

	// List of apps installed on the player device matching our filter
	private List<string> installedApps = new List<string>();

	// Contains information about the adverts we have available to be displayed and their statuses
	public List<AdJsonFileData> advertData = new List<AdJsonFileData>();

	// The textures are in a separate list so we can serialize the advertData to save it across sessions
	public List<Texture> advertTextures = new List<Texture>();

	public bool useStorageCache = true; // Should ads be downloaded to the device for use across sessions

	#if UNITY_EDITOR
	[ReadOnly] 
	#endif
	public bool logAdImpressions = true;
	#if UNITY_EDITOR
	[ReadOnly] 
	#endif
	public bool logAdClicks = true;

	public static Action OnIASImageDownloaded;
	public static Action OnForceChangeWanted;

	#if UNITY_EDITOR
		// When true the script checks for a new version when entering play mode
		public bool checkForLatestVersion = true;
	
		private IEnumerator CheckIASVersion()
		{
			WWW versionCheck = new WWW("http://data.i6.com/IAS/ias_check.txt");

			yield return versionCheck;

			int latestVersion = 0;

			int.TryParse(versionCheck.text, out latestVersion);

			if(latestVersion > internalScriptVersion){
				if(EditorUtility.DisplayDialog("IAS Update Available!", "There's a new version of the IAS script available!\nWould you like to update now?\n\nIAS files will be automatically replaced with their latest versions!", "Yes", "No")){
					string scriptPath = EditorUtility.OpenFilePanel("Select IAS_Manager.cs from your project!", "", "cs");

					if(scriptPath.Length > 0){
						// Remove assets from the path because Unity 5.4.x has a bug where the return value of the path doesn't include assets unlike other versions of unity
						scriptPath = scriptPath.Replace("Assets/", "");

						// Re-add Assets/ but also remove the data path so the path starts at Assets/
						scriptPath = scriptPath.Replace(Application.dataPath.Replace("Assets", ""), "Assets/");

						WWW scriptDownload = new WWW("http://data.i6.com/IAS/GamePickle/IAS_Manager.cs");

						yield return scriptDownload;

						FileStream tmpFile = File.Create(scriptPath + ".tmp");
						FileStream backupFile = File.Create(scriptPath + ".backup" + internalScriptVersion);

						tmpFile.Close();
						backupFile.Close();

						File.WriteAllText(scriptPath + ".tmp", scriptDownload.text);
						File.Replace(scriptPath + ".tmp", scriptPath, scriptPath + ".backup");
						File.Delete(scriptPath + ".tmp");

						// Update the AssetDatabase so we can see the file changes in Unity
						AssetDatabase.Refresh();

						Debug.Log("IAS upgraded from version " + internalScriptVersion + " to " + latestVersion);

						// Force exit play mode
						EditorApplication.isPlaying = false;
					} else {
						Debug.LogError("Update cancelled! Did not select the IAS_Manager.cs script!");
					}
				} else {
					Debug.LogError("Update cancelled! Make sure to update your IAS version before sending a build!");
				}
			}
		}
	#endif

	#if UNITY_ANDROID && ias
		private void UpdateInstalledPackages()
		{
			ProjectManager.AdvLog("IAS Updating Installed Packages");

			installedApps.Clear();

			// Get all installed packages with a bundleId matching our filter
			string filteredPackageList = I6MediaPlugin.GetPackageList("com.pickle.");

			// Cleanup the package list mistakes (ending comma or any spaces)
			if(!string.IsNullOrEmpty(filteredPackageList)){
				filteredPackageList = filteredPackageList.Trim(); // Trim whitespaces
				filteredPackageList = filteredPackageList.Remove(filteredPackageList.Length - 1); // Remove the unwanted comma at the end of the list

				// Split the list into a string array
				string[] packageArray = filteredPackageList.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

				if(packageArray.Length > 0){
					// Extract all packages and store them in the installedApps list
					foreach(string packageName in packageArray){
						installedApps.Add(packageName.Trim().ToLowerInvariant());
					}
				} else {
					Analytics.LogEvent("IAS", "No other installed packages found matching filter!");
				}
			} else {
				Analytics.LogEvent("IAS", "Filtered package list was empty!");
			}

			Analytics.SetUserProperty("ias_games_installed", installedApps.Count.ToString());
		}
	#endif

	void Awake()
	{
		staticRef = (staticRef == null ? this : staticRef);

		#if ias
			#if UNITY_5 && UNITY_ANDROID
				bundleId = Application.identifier;
				appVersion = Application.version;
			#else
				if(bundleId == "com.example.GameNameHere")
					Analytics.LogError("IAS", "bundleId has not been changed from default! This needs to be set for this game!");
			#endif

			#if UNITY_EDITOR
				if(checkForLatestVersion)
					StartCoroutine(CheckIASVersion());
			#endif
		#endif
	}

	#if ias
		void Start()
		{
			bool wasBundleIdLimited = bundleId.Length > 25;
			string bundleIdLimited = (wasBundleIdLimited ? bundleId.Substring(0, 25) : bundleId) + (wasBundleIdLimited ? ".." : "");

			Analytics.LogEvent("IAS", "Init", "[" + internalScriptVersion + "] " + bundleIdLimited  + " (" + appVersion + ")");

			#if UNITY_ANDROID
				// Get a list of installed packages on the device and store ones matching a filter
				UpdateInstalledPackages();
			#endif

			bool cachedIASDataLoaded = LoadIASData();

			StartCoroutine(DownloadIASData(cachedIASDataLoaded));

			// If there was some cached IAS data available refresh the ads now
			// The ads will also be refreshed once the IAS data reloads if any ad timestamps have changed
			if(cachedIASDataLoaded)
				RefreshActiveAdSlots();
		}

		private void RefreshActiveAdSlots()
		{
			// Refresh an ad for each slot int so they all have an active ad loaded and ready to be displayed
			for(int jsonFileId=0;DoesSlotFileIdExist(jsonFileId);jsonFileId++)
				for(int i=1;DoesSlotIntExist(jsonFileId, i);i++)
					RefreshBanners(jsonFileId, i);
		}

		private void RefreshActiveAdSlots(int jsonFileId)
		{
			for(int i=1;DoesSlotIntExist(jsonFileId, i);i++)
				RefreshBanners(jsonFileId, i);
		}

		private void RandomizeAdSlots(int jsonFileId)
		{
			for(int i=1;DoesSlotIntExist(jsonFileId, i);i++)
			{
				AdSlotData curSlotData = GetAdSlotData(jsonFileId, i);

				curSlotData.lastSlotId = UnityEngine.Random.Range(0, curSlotData.advert.Count-1);
			}
		}

		private string EncodeIASData()
		{
			try {
				BinaryFormatter binaryData = new BinaryFormatter();
				MemoryStream memoryStream = new MemoryStream();

				// Serialize our data list into the memory stream
				binaryData.Serialize(memoryStream, (object)advertData);

				string base64Data = string.Empty;

				try {
					// Convert the buffer of the memory stream (the serialized object) into a base 64 string
					base64Data = Convert.ToBase64String(memoryStream.GetBuffer());
				} catch(FormatException e){
					Analytics.LogError("IAS", "Advert data was corrupted! Could not convert to Base64! " + e.Message);
					throw;
				}

				return base64Data;
			} catch(SerializationException e){
				Analytics.LogError("IAS", "Encoding advert data serialization failed! " + e.Message);
				throw;
			}
		}

		private List<AdJsonFileData> DecodeIASData(string rawBase64Data)
		{
			try {
				BinaryFormatter binaryData = new BinaryFormatter();
				MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(rawBase64Data));

				try {
					return (List<AdJsonFileData>)binaryData.Deserialize(memoryStream);
				} catch(SerializationException e){
					Analytics.LogError("IAS", "Decoding advert data serialization failed! " + e.Message);
					throw;
				}
			} catch(FormatException e){
				Analytics.LogError("IAS", "Saved advert data was corrupted! " + e.Message);
				throw;
			}
		}

		private void SaveIASData()
		{
			// Make sure the advertData has actually been setup before trying to save it
			if(advertData != null){
				string iasData = EncodeIASData();

				PlayerPrefs.SetString("IASAdvertData", iasData);
			}
		}

		private bool LoadIASData()
		{
			string loadedIASData = PlayerPrefs.GetString("IASAdvertData", string.Empty);

			if(!string.IsNullOrEmpty(loadedIASData)){
				advertData = DecodeIASData(loadedIASData);

				// Some parts of the data needs their values changing as they're no longer valid for this session
				foreach(AdJsonFileData curFileData in advertData)
				{
					foreach(AdSlotData curSlotData in curFileData.slotInts)
					{
						foreach(AdData curData in curSlotData.advert)
						{
							curData.adTextureId = -1;
							curData.isTextureReady = false;
						}
					}
				}

				if(advertData != null){
					return true;
				}
			}

			return false;
		}

		private bool IsPackageInstalled(string packageName)
		{
			foreach(string comparisonApp in installedApps)
				if(packageName.ToLowerInvariant().Contains(comparisonApp))
					return true;

			return false;
		}

		private bool DoesSlotFileIdExist(int jsonFileId)
		{
			return ((jsonFileId >= advertData.Count) ? false : true);
		}

		private bool DoesSlotIntExist(int jsonFileId, int wantedSlotInt)
		{
			return ((GetAdSlotData(jsonFileId, wantedSlotInt) == null) ? false : true);
		}

		private bool DoesSlotCharExist(int jsonFileId, int wantedSlotInt, char wantedSlotChar)
		{
			return ((GetAdData(jsonFileId, wantedSlotInt, wantedSlotChar) == null) ? false : true);
		}

		private int GetSlotIndex(int jsonFileId, int wantedSlotInt)
		{
			if(DoesSlotFileIdExist(jsonFileId) && advertData[jsonFileId].slotInts != null){
				// Iterate through each slot in the requested json file
				for(int i=0;i < advertData[jsonFileId].slotInts.Count;i++)
				{
					AdSlotData curSlotData = advertData[jsonFileId].slotInts[i];

					// Check if this ad slot int matched the one we requested
					if(curSlotData.slotInt == wantedSlotInt)
						return i;
				}
			}

			return -1;
		}

		private int GetAdIndex(int jsonFileId, int wantedSlotInt, char wantedSlotChar)
		{
			AdSlotData slotData = GetAdSlotData(jsonFileId, wantedSlotInt);

			if(slotData.advert != null){
				for(int i=0;i < slotData.advert.Count;i++)
				{
					AdData curAdData = slotData.advert[i];

					if(wantedSlotChar == curAdData.slotChar)
						return i;
				}
			}

			return -1;
		}

		private AdSlotData GetAdSlotData(int jsonFileId, int wantedSlotInt)
		{
			if(DoesSlotFileIdExist(jsonFileId) && advertData[jsonFileId].slotInts != null){
				// Iterate through each slot in the requested json file
				foreach(AdSlotData curSlotData in advertData[jsonFileId].slotInts)
				{
					// Check if this ad slot int matches the one we requested
					if(curSlotData.slotInt == wantedSlotInt)
						return curSlotData;
				}
			}

			return null;
		}

		private AdData GetAdData(int jsonFileId, int wantedSlotInt)
		{
			return GetAdData(jsonFileId, wantedSlotInt, GetSlotChar(jsonFileId, wantedSlotInt));
		}

		private AdData GetAdData(int jsonFileId, int wantedSlotInt, char wantedSlotChar)
		{
			AdSlotData curAdSlotData = GetAdSlotData(jsonFileId, wantedSlotInt);

			if(curAdSlotData != null){
				foreach(AdData curData in curAdSlotData.advert)
				{
					// Check if this ad slot character matches the one we requested
					if(curData.slotChar == wantedSlotChar)
						return curData;
				}
			}

			return null;
		}

		private void IncSlotChar(int jsonFileId, int wantedSlotInt)
		{
			AdSlotData wantedSlotData = GetAdSlotData(jsonFileId, wantedSlotInt);

			bool isValidAd = false;
			AdData curAdData = null;

			int adSlotCount = wantedSlotData.advert.Count;

			for(int i=0;!isValidAd && i < (adSlotCount*2);i++){
				wantedSlotData.lastSlotId = wantedSlotData.lastSlotId + 1 >= adSlotCount ? 0 : wantedSlotData.lastSlotId + 1;

				curAdData = GetAdData(jsonFileId, wantedSlotInt, GetSlotChar(jsonFileId, wantedSlotInt));

				// Never display any self ads or inactive ads
				if(!curAdData.isSelf && curAdData.isActive){
					// Only apps which are not already installed are valid UNLESS we iterated through all ads and could not find a valid one
					if(!curAdData.isInstalled || i >= adSlotCount){
						isValidAd = true;
					}
				}
			}

			if(isValidAd)
				StartCoroutine(DownloadAdTexture(curAdData));
		}

		private char GetSlotChar(int jsonFileId, int wantedSlotInt)
		{
			AdSlotData curSlotData = GetAdSlotData(jsonFileId, wantedSlotInt);

			if(curSlotData != null){
				return (char)(curSlotData.lastSlotId + slotIdDecimalOffset);
			} else {
				return default(char);
			}
		}

		private IEnumerator DownloadIASData(bool cachedDataLoaded = false)
		{
			// Wait for an active internet connection
			while(Application.internetReachability == NetworkReachability.NotReachable)
				yield return null;

			ProjectManager.AdvLog("IAS downloading data..");

			// Iterate through each JSON file
			for(int jsonFileId=0;jsonFileId < jsonUrls.Length;jsonFileId++)
			{
				// Download the JSON file
				WWW wwwJSON = new WWW (jsonUrls[jsonFileId]);

				// Wait for the JSON data to be downloaded
				yield return wwwJSON;

				// Check for any errors
				if(!string.IsNullOrEmpty(wwwJSON.error)){
					Analytics.LogError("IAS", "JSON download error! " + wwwJSON.error);
					yield break;
				} else if(wwwJSON.text.Contains("There was an error")){
					Analytics.LogError("IAS", "JSON download error! Serverside system error!");
					yield break;
				} else if(string.IsNullOrEmpty(wwwJSON.text)){
					Analytics.LogError("IAS", "JSON download error! Empty JSON!");
					yield break;
				}

				JsonFileData tempAdvertData = JsonUtility.FromJson<JsonFileData>(wwwJSON.text);

				// Dispose of the wwwJSON data (clear it from memory)
				wwwJSON.Dispose();

				if(tempAdvertData == null){
					Analytics.LogError("IAS", "Temp advert data was null!");
					yield break;
				}

				if(!DoesSlotFileIdExist(jsonFileId))
					advertData.Add(new AdJsonFileData());

				bool needToReloadAdSlots = !cachedDataLoaded;
				bool needToRandomizeSlot = false;

				// We're currently only using the slots, not containers
				for(int i=0;i < tempAdvertData.slots.Count;i++)
				{
					JsonSlotData curSlot = tempAdvertData.slots[i];

					// We'll be converting the slot id (e.g 1a, 1c or 2f) into just number and just character values
					int slotInt; char slotChar;

					// Attempt to extract the slot int from the slot id
					if(!int.TryParse(Regex.Replace(curSlot.slotid, "[^0-9]", ""), out slotInt)){
						Analytics.LogError("IAS", "Failed to parse slot int from '" + curSlot.slotid + "'");
						yield break;
					}

					// Attempt to extract the slot character from the slot id
					if(!char.TryParse(Regex.Replace(curSlot.slotid, "[^a-z]", ""), out slotChar)){
						Analytics.LogError("IAS", "Failed to parse slot char from '" + curSlot.slotid + "'");
						yield break;
					}

					// If this slot doesn't exist yet create a new slot for it
					if(!DoesSlotIntExist(jsonFileId, slotInt))
						advertData[jsonFileId].slotInts.Add(new AdSlotData(slotInt, new List<AdData>()));

					// Get the index in the list for slotInt
					int slotDataIndex = GetSlotIndex(jsonFileId, slotInt);

					if(slotDataIndex < 0){
						Analytics.LogError("IAS", "Failed to get slotDataIndex!");
						yield break;
					}

					// Make sure this slot char isn't repeated in the json file within this slot int for some reason
					if(!DoesSlotCharExist(jsonFileId, slotInt, slotChar)){
						advertData[jsonFileId].slotInts[slotDataIndex].advert.Add(new AdData(slotChar));
						needToRandomizeSlot = true;
					}

					int slotAdIndex = GetAdIndex(jsonFileId, slotInt, slotChar);

					if(slotAdIndex < 0){
						Analytics.LogError("IAS", "Failed to get slotAdIndex! Could not find " + slotInt + ", " + slotChar.ToString());
						yield break;
					}

					AdData curAdData = advertData[jsonFileId].slotInts[slotDataIndex].advert[slotAdIndex];

					// Extract the bundleId of the advert
					#if UNITY_ANDROID
						// Regex extracts the id GET request from the URL which is the package name of the game
						// (replaces everything that does NOT match id=blahblah END or NOT match id=blahblah AMERPERSAND
						string packageName = Regex.Match(curSlot.adurl, "(?<=id=)((?!(&|\\?)).)*").Value;
					#elif UNITY_IOS
						// IOS we just need to grab the name after the hash in the URL
						string packageName = Regex.Match(curSlot.adurl, "(?<=.*#).*").Value;
					#else
						// For other platforms we should be fine to just use the full URL for package name comparisons as we'll be using .Compare
						// And other platforms won't include any other referral bundle ids in their URLs
						string packageName = curSlot.adurl;
					#endif

					string imageFileType = Regex.Match(curSlot.imgurl, "(?<=/uploads/adverts/.*)\\.[A-z]*[^(\\?|\")]").Value;

					curAdData.fileName = curSlot.slotid + imageFileType;
					curAdData.isSelf = packageName.Contains(bundleId);
					curAdData.isActive = curSlot.active;
					curAdData.isInstalled = IsPackageInstalled(packageName);
					curAdData.adUrl = curSlot.adurl;
					curAdData.packageName = packageName;

					curAdData.imgUrl = curSlot.imgurl;

					if(curAdData.newUpdateTime < curSlot.updatetime || curAdData.newUpdateTime == 0L)
						needToReloadAdSlots = true;

					curAdData.newUpdateTime = curSlot.updatetime;

					// I'm not pre-downloading all the images here because it takes quite a long time to download even on our fast ethernet connection (~15 seconds)
					// So I think it's best to download the images (if needed) when the ads are called to be refreshed
				}

				if(needToRandomizeSlot)
					RandomizeAdSlots(jsonFileId);

				if(needToReloadAdSlots)
					RefreshActiveAdSlots(jsonFileId);
			}

			SaveIASData();

			ProjectManager.AdvLog("IAS Done");
		}

		private IEnumerator DownloadAdTexture(AdData curAdData)
		{
			// Download the texture for the newly selected IAS advert
			// Only bother re-downloading the image if the timestamp has changed or the texture isn't marked as ready
			if(!curAdData.isTextureReady || curAdData.lastUpdated < curAdData.newUpdateTime){
				// Check if this is an advert we may be using in this game
				// Note: We still download installed ads because we might need them if there's no ads to display
				if(!curAdData.isSelf && curAdData.isActive){
					// Whilst we still have wwwImage write the bytes to disk to save on needing extra operations
					string filePath = Application.persistentDataPath + Path.AltDirectorySeparatorChar; 

					string fileName = "IAS_" + curAdData.fileName;

					// Check to see if we have this advert locally cached
					if(curAdData.isTextureFileCached){
						// Make sure the cache file actually exists (unexpected write fails or manual deletion)
						if(File.Exists(filePath + fileName)){
							try {
								// Read the saved texture from disk
								byte[] imageData = File.ReadAllBytes(filePath + fileName);

								// We need to create a template texture, we're also setting the compression type here
								Texture2D imageTexture = new Texture2D(2, 2, TextureFormat.ETC2_RGBA1, false);

								// Load the image data, this will also resize the texture
								imageTexture.LoadImage(imageData);

								advertTextures.Add(imageTexture);
							} catch(IOException e){
								Analytics.LogError("IAS", "Failed to load cached file! " + e.Message);
								curAdData.isTextureFileCached = false;
								throw;
							}
						} else {
							Analytics.LogError("IAS", "Saved cached image was missing!");
							curAdData.isTextureFileCached = false;
							yield break;
						}
					} else {
						// The advert is not yet locally cached, 
						WWW wwwImage = new WWW(curAdData.imgUrl);

						// Wait for the image data to be downloaded
						yield return wwwImage;

						// Check for any errors
						if(!string.IsNullOrEmpty(wwwImage.error)){
							Analytics.LogError("IAS", "Image download error! " + wwwImage.error);
							yield break;
						} else if(wwwImage.text.Contains("There was an error")){
							Analytics.LogError("IAS", "Image download error! Serverside system error!");
							yield break;
						} else if(string.IsNullOrEmpty(wwwImage.text)){
							Analytics.LogError("IAS", "Image download error! Empty JSON!");
							yield break;
						}

						advertTextures.Add(wwwImage.texture);

						try {
							File.WriteAllBytes(filePath + fileName, wwwImage.bytes);
							curAdData.isTextureFileCached = true;
						} catch(IOException e){
							Analytics.LogError("IAS", "Failed to create cache file! " + e.Message);
							throw;
						}

						// Dispose of the wwwImage data as soon as we no longer need it (clear it from memory)
						wwwImage.Dispose();
					}

					curAdData.adTextureId = advertTextures.Count - 1;
					curAdData.lastUpdated = curAdData.newUpdateTime;
					curAdData.isTextureReady = true;
				}
			}

			OnIASImageDownloaded();
		}
	#endif

	// Save the IAS data as the user quit the app (as saving whenever the data is updated is expensive)
	// OnApplicationQuit isn't always called as the player may just minimize then kill the app when
	// Or on iOS the app is suspended (calling OnApplicationPause(true)) unless "Exit on suspend" is enabled
	void OnApplicationQuit()
	{
		SaveIASData();
	}

	void OnApplicationFocus(bool focusState)
	{
		if(!focusState)
			SaveIASData();
	}

	void OnApplicationPause(bool pauseState)
	{
		if(pauseState)
			SaveIASData();
	}

	/// <summary>
	/// Call this for every IAS advert the player views
	/// </summary>
	/// <param name="url">Advert URL</param>
	public static void OnImpression(string packageName)
	{
		#if ias
			if(staticRef.logAdImpressions){
				Analytics.LogEvent("IAS", "Impressions", packageName);

				// Replace com.pickle. in the package name if it exists (otherwise the length will just be limited within SetUserProperty)
				Analytics.SetUserProperty("last_ias_impression", packageName.Replace("com.pickle.", ""));
			}
		#endif
	}

	/// <summary>
	/// Call this for every IAS advert the player clicks
	/// </summary>
	/// <param name="url">Advert URL</param>
	public static void OnClick(string packageName)
	{
		#if ias
			if(staticRef.logAdClicks){
				Analytics.LogEvent("IAS", "Clicks", packageName);

				// Replace com.pickle. in the package name if it exists (otherwise the length will just be limited within SetUserProperty)
				Analytics.SetUserProperty("last_ias_click", packageName.Replace("com.pickle.", ""));
			}
		#endif
	}

	/// <summary>
	/// Refreshes the IAS adverts
	/// </summary>
	/// <param name="jsonFileId">JSON file ID</param>
	/// <param name="wantedSlotInt">Slot int</param>
	public static void RefreshBanners(int jsonFileId, int wantedSlotInt, bool forceChangeActive = false)
	{
		#if ias
			staticRef.IncSlotChar(jsonFileId, wantedSlotInt);

			if(forceChangeActive)
				OnForceChangeWanted();
		#endif
	}

	/// <summary>
	/// Returns whether the ad texture has downloaded or not
	/// </summary>
	/// <returns><c>true</c> if is ad ready the specified jsonFileId wantedSlotInt; otherwise, <c>false</c>.</returns>
	/// <param name="jsonFileId">JSON file ID</param>
	/// <param name="wantedSlotInt">Slot int</param>
	public static bool IsAdReady(int jsonFileId, int wantedSlotInt)
	{
		#if ias
			AdData returnValue = staticRef.GetAdData(jsonFileId, wantedSlotInt);

			if(returnValue != null){
				return returnValue.isTextureReady;
			} else {
				return false;
			}
		#else
			return false;
		#endif
	}

	/// <summary>
	/// Returns the URL of the current active advert from the requested JSON file and slot int
	/// </summary>
	/// <returns>The advert URL</returns>
	/// <param name="jsonFileId">JSON file ID</param>
	/// <param name="wantedSlotInt">Slot int</param>
	public static string GetAdURL(int jsonFileId, int wantedSlotInt)
	{
		#if ias
			AdData returnValue = staticRef.GetAdData(jsonFileId, wantedSlotInt);

			if(returnValue != null){
				return returnValue.adUrl;
			} else {
				return string.Empty;
			}
		#else
			return string.Empty;
		#endif
	}

	/// <summary>
	/// Returns the package name of the current active advert from the requested JSON file and slot int
	/// </summary>
	/// <returns>The advert package name</returns>
	/// <param name="jsonFileId">JSON file ID</param>
	/// <param name="wantedSlotInt">Slot int</param>
	public static string GetAdPackageName(int jsonFileId, int wantedSlotInt)
	{
		#if ias
			AdData returnValue = staticRef.GetAdData(jsonFileId, wantedSlotInt);

			if(returnValue != null){
				return returnValue.packageName;
			} else {
				return string.Empty;
			}
		#else
			return string.Empty;
		#endif
	}

	/// <summary>
	/// Returns the Texture of the current active advert from the requested JSON file and slot int
	/// </summary>
	/// <returns>The advert texture</returns>
	/// <param name="jsonFileId">JSON file ID</param>
	/// <param name="wantedSlotInt">Slot int</param>
	public static Texture GetAdTexture(int jsonFileId, int wantedSlotInt)
	{
		#if ias
			AdData returnValue = staticRef.GetAdData(jsonFileId, wantedSlotInt);

			if(returnValue != null){
				return staticRef.advertTextures[returnValue.adTextureId];
			} else {
				return null;
			}
		#else
			return null;
		#endif
	}

}
