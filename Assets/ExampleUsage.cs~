using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if database
using Firebase.Database;
#endif

public class ExampleUsage : MonoBehaviour {

	void OnEnable()
	{
		Database.OnDBModified += OnDBModified;
		Database.OnDBSetComplete += OnDBSet;
	}

	void OnDisable()
	{
		// Cleanup the callbacks if the gameobject is disabled
		Database.OnDBModified -= OnDBModified;
		Database.OnDBSetComplete -= OnDBSet;
	}

	void Update()
	{
		if (Input.GetKeyDown (KeyCode.U)) {
			//Database.Set(new string[]{"Room0", "Chat"
		}
	}

	void OnDBModified(string modifiedKey)
	{
		#if database
		DataSnapshot snapshot = Database.GetDataSnapshot(modifiedKey);

		Debug.Log("Key: " + snapshot.Key + ", Value: " + snapshot.Value + " was just modified!");
		#endif
	}

	void OnDBSet(string modifiedKey)
	{
		// Note that the snapshot is either or outdated or doesn't exist at this point until a get is called! (which is done automatically unless updateLocalDB is false when calling Set)
		Debug.Log ("DB set value for " + modifiedKey);
	}

}
