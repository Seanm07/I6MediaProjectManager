using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class iOSBillingEvents : MonoBehaviour {

	public static void DidReceiveBillingProducts(string dataStr)
	{
		IDictionary dataDict = (IDictionary)JsonUtility.FromJson(dataStr);
		string error = dataDict.GetIfAvailable<string>("error");

		if(!string.IsNullOrEmpty(error)){
			DidReceiveBillingProducts(null, error);
			return;
		} else {
			IList regProductsJSONList = dataDict.GetIfAvailable<IList>("products");
			ItemData[] regProductsList = null;

			if(regProductsJSONList != null){
				regProductsList = new iosItemData[regProductsJSONList.Count];
				int i = 0;

				foreach(IDictionary productInfoDict in regProductsJSONList)
				{
					regProductsList[i++] = new iosItemData(productInfoDict);
				}
			}

			DidReceiveBillingProducts(regProductsList, null);
			return;
		}
	}

	public static void ExtractTransactionResponseData(string dataStr, out ItemTransaction[] transactions, out string error)
	{
		// Set default values
		transactions = null;
		error = null;

		// Parse and fetch properties from JSON object
		IDictionary dataDict = (IDictionary)JsonUtility.FromJson(dataStr);
		error = dataDict.GetIfAvailable<string>("error");

		if(string.IsNullOrEmpty(error)){
			IList transactionsJSONList = dataDict.GetIfAvailable<IList>("transactions");

			if(transactionsJSONList != null){
				int count = transactionsJSONList.Count;
				transactions = new iosItemData[count];

				for(int i=0;i < count;i++)
					transactions[i] = new iosItemData((IDicti
			}
		}
	}
}
