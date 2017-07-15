/*
 * Last updated 2nd June 2017
 * Written by Sean McManus for i6 Media sean@i6.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if messaging
	using Firebase.Messaging;
#endif

public class Messaging : MonoBehaviour {

	#if messaging
		void OnEnable()
		{
			FirebaseMessaging.TokenReceived += OnTokenReceived;
			FirebaseMessaging.MessageReceived += OnMessageReceived;
		}

		void OnDisable()
		{
			FirebaseMessaging.TokenReceived -= OnTokenReceived;
			FirebaseMessaging.MessageReceived -= OnMessageReceived;
		}

		void OnTokenReceived(object sender, TokenReceivedEventArgs token)
		{
			Analytics.LogEvent ("Received Registration Token", token.Token);

			Debug.Log ("Received registration token!" + token.Token);
		}

		void OnMessageReceived(object sender, MessageReceivedEventArgs e)
		{
			Analytics.LogEvent ("Received Message", e.Message.From);

			Debug.Log ("Received message: " + e.Message.From);
		}
	#endif

	public static void Subscribe(string topic)
	{
		#if messaging
			FirebaseMessaging.Subscribe (topic);
		#endif
	}

	public static void Unsubscribe(string topic)
	{
		#if messaging
			FirebaseMessaging.Unsubscribe (topic);
		#endif
	}

}
