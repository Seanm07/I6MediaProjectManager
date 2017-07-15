﻿/*
 * Last updated 2nd June 2017
 * Written by Sean McManus for i6 Media sean@i6.com
 * 
 * The script isn't completely fleshed out but it contains all the basic functionality which would be required to use the Database
 * More useful functions are probably required if used for realtime multiplayer etc
 * Contact me and let me know about any additions you make and I can work them into the live version!
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if database
	using Firebase;
	using Firebase.Unity.Editor;
	using Firebase.Database;
#endif

public class Database : MonoBehaviour {

	public static Database staticRef;

	public string databaseURL;

	#if database
		public static FirebaseDatabase dbInstance;
		public static DatabaseReference dbReference;

		// This is effectively a local copy of the database which will only update as we get / set data as required
		private Dictionary<string, DataSnapshot> localDB = new Dictionary<string, DataSnapshot>();
	#endif

	// Called when any changes to the localDB have been made (meaning we can instantly use whatever data is in there)
	public static event Action<string> OnDBModified;

	// Called when a db set action completes (not the local DB, need to Get(..) any data afterwards - note that Get(..) is automatically called from a Set(..) unless updateLocalDB is set to false)
	public static event Action<string> OnDBSetComplete; 

	#if database
		void Awake()
		{
			staticRef = (staticRef == null ? this : staticRef);
		}

		public static void Initialize()
		{
			FirebaseApp.DefaultInstance.SetEditorDatabaseUrl (staticRef.databaseURL);

			dbInstance = FirebaseDatabase.DefaultInstance;
			dbReference = dbInstance.RootReference;
		}

		/*void Update()
		{
			if (Input.GetKeyDown (KeyCode.O)) {
				Set (new string[]{"AnotherFolder", "New"}, "Hello World!");
			}

			if (Input.GetKeyDown (KeyCode.P)) {
				Get (new string[]{ "Room0", "Chat", "m023423" });
			}
		}*/

		private DatabaseReference GetDBRef(string[] dbPath)
		{
			DatabaseReference curDbPath = dbReference;

			foreach (string path in dbPath)
				curDbPath = curDbPath.Child (path);

			return curDbPath;
		}

		/// <summary>
		/// Returns a data snapshot from the localDB
		/// </summary>
		/// <returns>The data snapshot</returns>
		/// <param name="key">Database reference key</param>
		public static DataSnapshot GetDataSnapshot(string key)
		{
			return staticRef.localDB[key];
		}

		private static string DBRefToKey(DatabaseReference dbRef)
		{
			return dbRef.ToString ().ToLowerInvariant ();
		}
			
		private void OnGetData(DataSnapshot receievedData)
		{
			string dbReferenceKey = DBRefToKey(receievedData.Reference); // Reference to where we are in the database

			if (localDB.ContainsKey (dbReferenceKey)) {
				localDB [dbReferenceKey] = receievedData;
			} else {
				localDB.Add (dbReferenceKey, receievedData);
			}

			OnDBModified (dbReferenceKey);
		}

		/// <summary>
		/// Get data from the database at the specified path
		/// </summary>
		/// <param name="dbPath">Data path within the database</param>
		public static string Get(string[] dbPath)
		{
			DatabaseReference dbRef = staticRef.GetDBRef (dbPath);
			string dbRefKey = DBRefToKey (dbRef);

			dbRef.GetValueAsync ().ContinueWith (task => {
				if(task.IsCanceled){
					Analytics.LogError("Firebase Get Database", "Get database canceled!");
					return;
				}

				if(task.IsFaulted){
					Analytics.LogError("Firebase Get Database", "Error: " + task.Exception);
					return;
				}

				staticRef.OnGetData(task.Result);
			});

			// Return the dbRefKey so we know what we're waiting for to complete with the callbacks
			return dbRefKey;
		}

		/// <summary>
		/// Set or create a new value in the database. If updateLocalDB is true then a Get request will be ran once the value has been set to update the localDB.
		/// </summary>
		/// <param name="dbPath">Path in database for the new value</param>
		/// <param name="newValue">Value the db key will be set to</param>
		/// <param name="updateLocalDB">If set to <c>true</c> a Get request will be called once the set completes to update the localDB</param>
		public static string Set(string[] dbPath, string newValue, bool updateLocalDB = true)
		{
			DatabaseReference dbRef = staticRef.GetDBRef (dbPath);
			string dbRefKey = DBRefToKey (dbRef);

			dbRef.SetValueAsync (newValue).ContinueWith (task => {
				if(task.IsCanceled){
					Analytics.LogError("Firebase Set Database", "Set database canceled!");
					return;
				}

				if(task.IsFaulted){
					Analytics.LogError("Firebase Set Database", "Error: " + task.Exception);
					return;
				}
					
				if(updateLocalDB){
					// Do a get request for this dbPath as we need to update the DataSnapshot we have stored
					Get(dbPath);
				}

				OnDBSetComplete(dbRefKey);
			});

			// Return the dbRefKey so we know what we're waiting for to complete with the callbacks
			return dbRefKey;
		}
	#else
		// Dummy functions so references to this script don't throw errors when the database is disabled
		public static void Initialize(){}
		public static object GetDataSnapshot(string key){ return null; }
		public static string Get(string[] dbPath) { return ""; }
		public static string Set(string[] dbPath, string newValue, bool updateLocalDB = true) { return ""; }
	#endif
}
