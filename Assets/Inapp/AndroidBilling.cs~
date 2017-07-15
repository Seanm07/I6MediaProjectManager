using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidBilling : MonoBehaviour {

	private static AndroidJavaObject Plugin;

	public static void Initialize(string publicKey, ItemData[] itemList)
	{
		Plugin = AndroidPluginUtility.GetSingletonInstance("com.voxelbusters.nativeplugins.features.billing.BillingHandler");

		// Get a list of consumable item ids and convert it to a json string
		string[] consumableItemIds = Billing.GetConsumableItemIds(itemList);

		Plugin.Call("initialize", publicKey, JsonUtility.ToJson(consumableItemIds));
	}

	public static bool IsReady()
	{
		return (Plugin != null);
	}

	public static bool IsInitialized()
	{
		return IsReady() && Plugin.Call<bool>("isInitialized");
	}

	public static void Purchase(string itemId)
	{
		if(!string.IsNullOrEmpty(itemId)){
			Plugin.Call("buyProduct", itemId);
		} else {
			Debug.LogError("Could not purchase item with invalid item ID!");
		}
	}

	public static void Purchase(string itemId, string developerPayload)
	{
		if(!string.IsNullOrEmpty(itemId)){
			Plugin.Call("buyProduct", itemId, developerPayload);
		} else {
			Debug.LogError("Could not purchase item with invalid item ID!");
		}
	}
	
	public static bool IsItemOwned(string itemId)
	{
		if(!string.IsNullOrEmpty(itemId)){
			return Plugin.Call<bool>("isProductPurchased", itemId);
		} else {
			Debug.LogError("Could not check if item was owned with invalid item ID!");
		}

		return false;
	}

	public static void RestorePurchases()
	{
		Plugin.Call("restoreCompletedTransactions");
	}
}
