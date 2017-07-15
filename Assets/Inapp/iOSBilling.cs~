using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class iosItemData : ItemData {
	private const string cTitle = "localized-title";
	private const string cProductId = "product-identifier";
	private const string cDescription = "localized-description";
	private const string cPrice = "price";
	private const string cLocalizedPrice = "localized-price";
	private const string cCurrencyCode = "currency-code";
	private const string cCurrencySymbol = "currency-symbol";

	public iosItemData(IDictionary productJsonDict){
		name = productJsonDict.GetIfAvailable<string>(cTitle);
		itemId = productJsonDict.GetIfAvailable<string>(cProductId);
		desc = productJsonDict.GetIfAvailable<string>(cDescription);
		price = productJsonDict.GetIfAvailable<float>(cPrice);
		localizedPrice = productJsonDict.GetIfAvailable<string>(cLocalizedPrice);
		currencyCode = productJsonDict.GetIfAvailable<string>(cCurrencyCode);
		currencySymbol = productJsonDict.GetIfAvailable<string>(cCurrencySymbol);
	}

	public static IDictionary CreateJSONObject(ItemData item)
	{
		IDictionary itemJsonDict = new Dictionary<string, string>();

		itemJsonDict[cTitle] = item.name;
		itemJsonDict[cProductId] = item.itemId;
		itemJsonDict[cDescription] = item.desc;
		itemJsonDict[cPrice] = item.price;
		itemJsonDict[cLocalizedPrice] = item.localizedPrice;
		itemJsonDict[cCurrencyCode] = item.currencyCode;
		itemJsonDict[cCurrencySymbol] = item.currencySymbol;

		return itemJsonDict;
	}
}

public class iOSBilling : MonoBehaviour {

	[DllImport("__Internal")]
	private static extern void cpnpBillingInit (bool supportsReceiptValidation, string validateUsingServerURL, string sharedSecret);

	[DllImport("__Internal")]
	private static extern void cpnpBillingRequestForBillingProducts (string consumableProductIds, string nonConsumableProductIds);

	[DllImport("__Internal")]
	private static extern bool cpnpBillingCanMakePayments();

	[DllImport("__Internal")]
	private static extern bool cpnpBillingIsProductPurchased (string productId);

	[DllImport("__Internal")]
	private static extern void cpnpBillingBuyProduct (string productId);

	[DllImport("__Internal")]
	private static extern void cpnpBillingRestoreCompletedTransactions();

	[DllImport("__Internal")]
	private static extern void cpnpBillingFinishCompletedTransactions (string transactionIDs, bool isRestoreType);

	public static void Initialize(string serverValidationURL = "")
	{
		cpnpBillingInit(!string.IsNullOrEmpty(serverValidationURL), serverValidationURL, null);
	}

	public static bool IsReady()
	{
		return true;
	}

	public static bool IsInitialized()
	{
		return cpnpBillingCanMakePayments();
	}

	public static void Purchase(ItemData item)
	{
		Purchase(item);
	}

	public static void Purchase (string itemId)
	{
		cpnpBillingBuyProduct(itemId);
	}

	public static bool IsItemOwned(string itemId)
	{
		return cpnpBillingIsProductPurchased(itemId);
	}

	public static void RestorePurchases()
	{
		cpnpBillingRestoreCompletedTransactions();
	}

	public static string[] GetConsumableItemIds(ItemData[] items)
	{
		return null;
	}
}
