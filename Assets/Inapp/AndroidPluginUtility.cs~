using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_ANDROID
public class AndroidPluginUtility
{
	static Dictionary<string, AndroidJavaObject> sSingletonInstances = new Dictionary<string, AndroidJavaObject>();

	// Assuming the class follows standard naming- "INSTANCE" for singleton objects
	public static AndroidJavaObject GetSingletonInstance(string _className, string _methodName = "getInstance")
	{
		AndroidJavaObject _instance;

		// Attempt to get a cached value from the dictionary
		sSingletonInstances.TryGetValue(_className, out _instance);
		
		if(_instance == null)
		{
			// Create instance
			AndroidJavaClass _class = new AndroidJavaClass(_className);

			// If the class doesn't exist, throw an error
			if(_class != null) {
				_instance = _class.CallStatic<AndroidJavaObject>(_methodName);

				// Add the new instance value for this class name key
				sSingletonInstances.Add(_className, _instance);
			} else {
				Debug.LogError("Class = " + _className + " not found!");
				return null;
			}
			
		}

		return _instance;
	}

	public static AndroidJavaClass CreateClassObject(string _className)
	{
		// Create instance
		AndroidJavaClass _class = new AndroidJavaClass(_className);

		// If the class doesn't exist, throw an error
		if(_class == null)
			Debug.LogError("Class = " + _className + " not found!");
	
		return _class;
	}	
}
#endif