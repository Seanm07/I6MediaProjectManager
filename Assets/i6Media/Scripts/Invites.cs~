/*
 * Last updated 2nd June 2017
 * Written by Sean McManus for i6 Media sean@i6.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if invites
	using Firebase.Invites;
#endif

public class Invites : MonoBehaviour {

	#pragma warning disable 0414 0067 // Supress the 'is never used' warning when invites are disabled
		// Called for each invite this user sends containing the invite code
		public static event Action<string> OnInviteSent;
	#pragma warning restore 0414 0067 // Supress the 'is never used' warning when auth is disabled

	#if invites
		void OnEnable()
		{		
			FirebaseInvites.InviteReceived += OnInviteReceived;
			FirebaseInvites.InviteNotReceived += OnInviteNotReceived;
			FirebaseInvites.ErrorReceived += OnErrorReceived;
		}

		void OnDisable()
		{
			FirebaseInvites.InviteReceived -= OnInviteReceived;
			FirebaseInvites.InviteNotReceived -= OnInviteNotReceived;
			FirebaseInvites.ErrorReceived -= OnErrorReceived;
		}

		void OnInviteReceived(object sender, InviteReceivedEventArgs e)
		{
			if (e.InvitationId != "") { 
				Analytics.LogEvent ("Invite received", "Invitation ID: " + e.InvitationId);
				FirebaseInvites.ConvertInvitationAsync (e.InvitationId).ContinueWith (task => {
					if(task.IsCanceled){
						Analytics.LogError("Firebase Convert Invite", "Convert Invitation canceled!");
						return;
					}

					if(task.IsFaulted){
						Analytics.LogError("Firebase Convert Invite", "Error: " + task.Exception);
						return;
					}
				});
			}

			if (e.DeepLink.ToString () != "") {
				Analytics.LogEvent ("Invite received", "Deep link: " + e.DeepLink);
			}
		}

		void OnInviteNotReceived(object sender, EventArgs e)
		{
			// No invite or deep link received on startup
			// From what I understand I think this will be the default callback
		}

		void OnErrorReceived(object sender, InviteErrorReceivedEventArgs e)
		{
			Analytics.LogError ("Firebase Invite Received", "Error: " + e.ErrorMessage);
		}
	#endif

	public static void SendInvite(string title, string message, string callToAction, string linkURL)
	{
		#if invites
			Invite newInvite = new Invite () {
				TitleText = title,
				MessageText = message,
				CallToActionText = callToAction,
				DeepLinkUrl = new Uri(linkURL)
			};

			FirebaseInvites.SendInviteAsync (newInvite).ContinueWith (task => {
				if(task.IsCanceled){
					Analytics.LogError("Firebase Send Invite", "SendInvite canceled!");
					return;
				}

				if(task.IsFaulted){
					Analytics.LogError("Firebase Send Invite", "Error: " + task.Exception);
					return;
				}

				foreach(string id in task.Result.InvitationIds)
					OnInviteSent(id);
			});
		#endif
	}
}
