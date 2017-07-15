using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData {
	public string name;
	public string desc;
	public bool isConsumable;
	public float price;
	public string localizedPrice; // Price prefixed with currency symbol
	public string currencyCode;
	public string currencySymbol;

	private string _iosItemId; // Item registration ID on the iOS store
	private string _androidItemId; // Item registration ID on the android store

	public string itemId {
		get {
			#if UNITY_ANDROID
				return _androidItemId;
			#else
				return _iosItemId;
			#endif
		}

		set {
			#if UNITY_ANDROID
				_androidItemId = value;
			#else
				_iosItemId = value;
			#endif
		}
	}

	private string developerPayload; // Optional data to let the store send back security info (Android only)
}

public class Billing : MonoBehaviour {

	public static ItemData[] requestedItems = null;
	public static ItemData[] storeItems = null;

	#if UNITY_ANDROID
		public static void Initialize(string publicKey, ItemData[] itemList)
		{
			if(!string.IsNullOrEmpty(publicKey)){
				if(itemList.Length > 0){
					AndroidBilling.Initialize(publicKey, itemList);
				} else {
					Debug.LogError("Could not initialize billing with 0 items!");
				}
			} else {
				Debug.LogError("Invalid billing public key!");
			}
		}
	#elif UNITY_IOS
		public static void Initialize(string serverValidationURL = "")
		{
			iOSBilling.Initialize(serverValidationURL);
		}
	#endif

	public static bool IsReady()
	{
		#if UNITY_ANDROID
			return AndroidBilling.IsReady();
		#elif UNITY_IOS
			return iOSBilling.IsReady();
		#endif
	}

	public static bool IsInitialized()
	{
		#if UNITY_ANDROID
			return AndroidBilling.IsInitialized();
		#elif UNITY_IOS
			return iOSBilling.IsInitialized();
		#endif
	}

	public static void Purchase(ItemData item)
	{
		Purchase(item.itemId);
	}

	public static void Purchase(string itemId, string developerPayload)
	{
		if(!string.IsNullOrEmpty(itemId)){
			#if UNITY_ANDROID
				AndroidBilling.Purchase(itemId, developerPayload);
			#elif UNITY_IOS
				// The developer payload is android only
				iOSBilling.Purchase(itemId);
			#endif
		} else {
			Debug.LogError("Could not purchase item with invalid item ID!");
		}
	}

	public static void Purchase(string itemId)
	{
		if(!string.IsNullOrEmpty(itemId)){
			#if UNITY_ANDROID
				AndroidBilling.Purchase(itemId);
			#elif UNITY_IOS
				iOSBilling.Purchase(itemId);
			#endif
		} else {
			Debug.LogError("Could not purchase item with invalid item ID!");
		}
	}

	public static bool IsItemOwned(string itemId)
	{
		if(!string.IsNullOrEmpty(itemId)){
			#if UNITY_ANDROID
				return AndroidBilling.IsItemOwned(itemId);
			#elif UNITY_IOS
				return iOSBilling.IsItemOwned(itemId);
			#endif
		} else {
			Debug.LogError("Could not check if item was owned with blank item ID!");
		}

		return false;
	}

	public static void RestorePurchases()
	{
		#if UNITY_ANDROID
			AndroidBilling.RestorePurchases();
		#elif UNITY_IOS
			iOSBilling.RestorePurchases();
		#endif
	}

	public static string[] GetConsumableItemIds(ItemData[] items)
	{
		List<string> consumableItemIdList = new List<string>();

		foreach(ItemData curItem in items)
		{
			if(curItem.isConsumable)
				consumableItemIdList.Add(curItem.itemId);
		}

		return consumableItemIdList.ToArray();
	}

}
