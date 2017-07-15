using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BillingTransactionState { PURCHASED, FAILED, RESTORED, REFUNDED }
public enum BillingTransactionVerificationState { NOT_CHECKED, SUCCESS, FAILED }

public class ItemTransaction {
	public string productIdentifier { get; protected set; }
	public System.DateTime transactionDateUTC { get; protected set; }
	public System.DateTime transactionDateLocal { get; protected set; }
	public string transactionIdentifier { get; protected set; }
	public string transactionReceipt { get; protected set; }
	public BillingTransactionState transactionState { get; protected set; }
	public BillingTransactionVerificationState verificationState { get; protected set; }
	public string error { get; protected set; }
	public string rawPurchaseData { get; protected set; }

	protected ItemTransaction(){}

	internal ItemTransaction(string error)
	{
		// Set properties
		this.productIdentifier = null;
		this.transactionDateUTC = System.DateTime.UtcNow;
		this.transactionDateLocal = System.DateTime.Now;
		this.transactionIdentifier = null;
		this.transactionReceipt = null;
		this.transactionState = BillingTransactionState.FAILED;
		this.verificationState = BillingTransactionVerificationState.FAILED;
		this.error = error;
		this.rawPurchaseData = null;
	}
}

public class BillingEvents : MonoBehaviour {

	public delegate void RequestForBillingProductsCompletion(ItemData[] items, string error);

	public delegate void BuyProductCompletion(ItemTransaction transaction);

	public delegate void RestorePurchaseCompletion(ItemTransaction[] transactions, string error);


	public static event RequestForBillingProductsCompletion DidFinishRequestForBillingProductsEvent;

	public static event BuyProductCompletion DidFinishProductPurchaseEvent;

	public static event RestorePurchaseCompletion DidFinishRestoringPurchasesEvent;


	protected virtual void DidReceiveBillingProducts(string dataStr){
		#if UNITY_ANDROID
			//AndroidBillingEvents.DidReceieveBillingProducts(dataStr);
		#elif UNITY_IOS
			iOSBillingEvents.DidReceiveBillingProducts(dataStr);
		#endif

		// TODO: This is not yet finished, need to finish adding all the callback events 
		// for android and ios then test them

		// Note with the ios plugins we think that it may be due to the plugins being included in xcode
		// and those .h files are just the headers which tell it which of the classes to use
		// and explains the methods within the classes to xcode when it builds them.

		// TODO: See Notepad++ and the VoxelBusterCleanup Unity project for more info
		// The Billing files are partial which means there are Billing.cs Billing.Events.cs etc
		// which all work together under the Billing class so variables and functions can spread across them
	}


	protected void DidReceiveBillingProducts(ItemData[] storeItems, string error)
	{
		Debug.Log("Request for billing products finished successfull!");

		// Cache information
		Billing.storeItems = storeItems;

		// Update product type information
		if(storeItems != null){
			foreach(ItemData curItem in storeItems)
			{
				int productIndex = System.Array.FindIndex(Billing.requestedItems, (ItemData requestedItem) => curItem.itemId.Equals(requestedItem.itemId));

				if(productIndex != -1)
					curItem.isConsumable = Billing.requestedItems[productIndex].isConsumable;
			}
		}

		// Send event
		if(DidFinishRequestForBillingProductsEvent != null)
			DidFinishRequestForBillingProductsEvent(storeItems, error);
	}

	protected void DidFinishProductPurchase(string dataStr)
	{
		ItemTransaction[] transactions;
		string error;
		ExtractTransactionResponseData(dataStr, out transactions, out error);

		ProcessPurchaseTransactions(transactions);

		DidFinishProductPurchase(transactions, error);
	}

	protected void DidFinishProductPurchase(ItemTransaction[] transactions, string error)
	{
		Debug.Log("Received product purchase information!");

		// Send event
		if(DidFinishProductPurchaseEvent != null){
			if(error == null){
				foreach(ItemTransaction transaction in transactions)
					DidFinishProductPurchaseEvent(transaction);
			} else {
				DidFinishProductPurchaseEvent(new ItemTransaction(error));
			}
		}
	}

	protected void DidFinishRestoringPurchases(string dataStr)
	{
		ItemTransaction[] transactions;
		string error;

		ExtractTransactionResponseData(dataStr, out transactions, out error);

		ProcessRestoredTransactions(transactions);

		DidFinishRestoringPurchases(transactions, error);
	}

	protected void DidFinishRestoringPurchases(ItemTransaction[] transactions, string error)
	{
		Debug.Log("Received restored purchases information!");

		// Send event
		if(DidFinishRestoringPurchasesEvent != null)
			DidFinishRestoringPurchasesEvent(transactions, error);
	}

	protected virtual void ExtractTransactionResponseData(string dataStr, out ItemTransaction[] transactions, out string error)
	{
		transactions = null;
		error = null;
	}

	protected virtual void ProcessPurchaseTransactions(ItemTransaction[] transactions){ }
	protected virtual void ProcessRestoredTransactions(ItemTransaction[] transactions){ }

}
