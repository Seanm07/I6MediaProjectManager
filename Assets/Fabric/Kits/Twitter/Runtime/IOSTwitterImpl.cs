﻿/*
 * Copyright (C) 2015 Twitter, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

#if UNITY_IOS && !UNITY_EDITOR
namespace Fabric.Internal.Twitter
{
	using System;
	using System.Runtime.InteropServices;
	using UnityEngine;
	using Fabric.Twitter;
	
	internal class IOSTwitterImpl : ITwitter
	{
		[DllImport("__Internal")]
		private static extern void TwitterLogIn ();

		[DllImport("__Internal")]
		private static extern void TwitterLogOut ();

		[DllImport("__Internal")]
		private static extern string TwitterSession ();

		[DllImport("__Internal")]
		private static extern void TwitterCompose (string userID, string imageURI, string iPhoneAppID, string iPadAppID, string googlePlayAppID, string[] hashtags, int hashtagCount);

		[DllImport("__Internal")]
		private static extern void TwitterRequestEmail (string userID);

		public void LogIn ()
		{
			IOSTwitterImpl.TwitterLogIn ();
		}

		public void LogOut ()
		{
			IOSTwitterImpl.TwitterLogOut ();
		}

		public TwitterSession Session ()
		{
			string serializedSession = IOSTwitterImpl.TwitterSession();
			TwitterSession session = Fabric.Twitter.TwitterSession.Deserialize (serializedSession);
			return session;
		}

		public void RequestEmail (TwitterSession session)
		{
			IOSTwitterImpl.TwitterRequestEmail (Convert.ToString (session.id));
		}

		public void Compose (TwitterSession session, Card card, string[] hashtags)
		{
			IOSTwitterImpl.TwitterCompose (Convert.ToString(session.id), card.imageUri, card.appIPhoneId, card.appIPadId, card.appGooglePlayId, hashtags, hashtags.Length);
		}
	}
}
#endif
