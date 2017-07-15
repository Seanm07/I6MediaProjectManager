/*
 * Last updated 2nd June 2017
 * Written by Sean McManus for i6 Media sean@i6.com
 *
 * Note: The firebase registration, password changing, email verification and user deleting only work with firebase accounts registered with FirebaseRegister!
 * You obviously cannot modify third party auth stuff so keep that in mind when writing your scripts
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if auth
	#if analytics
		using Firebase.Analytics;
	#endif
	using Firebase;
	using Firebase.Auth;
	using System.Security.Cryptography;
	using System.Text;

	#if facebook
		using Facebook.Unity;
	#endif

	#if twitter
		using Fabric.Twitter;
	#endif

	#if google
		using GooglePlayGames;
		using GooglePlayGames.BasicApi;
		using UnityEngine.SocialPlatforms;
	#endif
#endif

public class Auth : MonoBehaviour {

	private static Auth staticRef;

	#if auth
		public static FirebaseAuth auth = null;
		public static FirebaseUser activeUser = null;

		private static bool isDownloadingPhoto;
		private static bool isPhotoDownloaded;
		private static Texture2D photoTexture; // Call GetPhotoTexture() to get the photoTexture

		#if facebook
			private AccessToken activeFacebookAccessToken;
		#endif

		#pragma warning disable 0414 // Supress the 'is never used' warning when facebook is disabled
			private float timescalePreFacebook = 1f; // Allows us to store the timescale before the facebook popup manually pauses the game
		#pragma warning restore 0414
	#endif

	public enum AuthType { Firebase, Google, Facebook, Twitter }

	public bool enableFacebookLoginAPI = false;
	public bool enableTwitterLoginAPI = false;
	public bool enableGooglePlayGamesAPI = false; // This is probably the most commonly used API

	#pragma warning disable 0414 0067 // Supress the 'is never used' warning when auth is disabled
		// Event called once the user avatar has finished downloading
		public static event Action OnPhotoReady;

		// Login events
		public static event Action<AuthErrorType> OnLoginFailed;
		public static event Action OnUserLogin;
		public static event Action OnUserLogout;

		// Registration events
		public static event Action<AuthErrorType> OnRegistrationFailed;
		public static event Action OnRegistrationSuccessful;

		// Password changing events
		public static event Action<AuthErrorType> OnPasswordChangeFailed;
		public static event Action OnPasswordChangeSuccessful;

		// Send email verification events
		public static event Action<AuthErrorType> OnSendEmailVerificationFailed;
		public static event Action OnSendEmailVerificationSuccessful;

		// User deletion events
		public static event Action<AuthErrorType> OnUserDeleteFailed;
		public static event Action OnUserDeleteSuccessful;

		// Get provider events (determines which auth the user account is linked to firebase with)
		public static event Action<AuthErrorType> OnGetProviderFailed;
		public static event Action OnGetProviderSuccessful;

		// Unlink auth events
		public static event Action<AuthErrorType> OnUnlinkAuthFailed;
		public static event Action OnUnlinkAuthSuccessful;

		public static event Action<string> OnTwitterEmailReceived;
	#pragma warning restore 0414 0067

	private bool hasInitialized = false;

	// These error codes are used in the Firebase C++ library, so I'm just implementing them myself as they didn't include them in the Unity version..
	// Or any proper error handling at all really T_T
	public enum AuthErrorType {
		ERROR_INVALID_CUSTOM_TOKEN,
		ERROR_CUSTOM_TOKEN_MISMATCH,
		ERROR_INVALID_CREDENTIAL,
		ERROR_INVALID_EMAIL,
		ERROR_WRONG_PASSWORD,
		ERROR_USER_MISMATCH,
		ERROR_REQUIRES_RECENT_LOGIN,
		ERROR_ACCOUNT_EXISTS_WITH_DIFFERENT_CREDENTIAL,
		ERROR_EMAIL_ALREADY_IN_USE,
		ERROR_CREDENTIAL_ALREADY_IN_USE,
		ERROR_USER_DISABLED,
		ERROR_USER_TOKEN_EXPIRED,
		ERROR_USER_NOT_FOUND,
		ERROR_INVALID_USER_TOKEN,
		ERROR_OPERATION_NOT_ALLOWED,
		ERROR_WEAK_PASSWORD,
		ERROR_UNKNOWN = -1
	}

	public string[] authErrorMessages = new string[]{
		"The custom token format is incorrect. Please check the documentation.",
		"The custom token corresponds to a different audience.",
		"The supplied auth credential is malformed or has expired.",
		"The email address is badly formatted.",
		"The password is invalid or the user does not have a password.",
		"The supplied credentials do not correspond to the previously signed in user.",
		"This operation is sensitive and requires recent authentication. Log in again before retrying this request.",
		"An account already exists with the same email address but different sign-in credentials. Sign in using a provider associated with this email address.",
		"The email address is already in use by another account.",
		"This credential is already associated with a different user account.",
		"The user account has been disabled by an administrator.",
		"The user's credential is no longer valid. The user must sign in again.",
		"There is no user record corresponding to this identifier. The user may have been deleted.",
		"The user's credential is no longer valid. The user must sign in again.",
		"This operation is not allowed. You must enable this service in the console.",
		"The given password is invalid."
	};

	void Awake()
	{
		staticRef = (staticRef == null ? this : staticRef);
	}

	// Firebase should really have something built in so I didn't need to do this..
	// If they add it in a future version this function may be removed
	private static AuthErrorType ConvertToAuthError(string errorMessage)
	{
		AuthErrorType errorType = AuthErrorType.ERROR_UNKNOWN;

		for(int i=0;i < staticRef.authErrorMessages.Length;i++)
		{
			if(errorMessage == staticRef.authErrorMessages[i]){
				errorType = (AuthErrorType)i;
			}
		}

		return errorType;
	}

	private static string ConvertToErrorMessage()
	{
		string errorMessage = "Unknown";

		// TODO: Finish this function so it's the reverse of the above

		//switch(
		return errorMessage;
	}

	#if auth
		/// <summary>
		/// Called from ProjectManager.cs when the firebase dependencies are resolved
		/// </summary>
		public static void Initialize()
		{
			auth = FirebaseAuth.DefaultInstance;

			#if facebook
				if (!FB.IsInitialized) {
					FB.Init (staticRef.FacebookInitComplete, staticRef.FacebookOnHideUnity);
				} else {
					staticRef.FacebookInitComplete();
				}
			#endif

			#if twitter
				Twitter.Init ();
			#endif

			#if google
				PlayGamesClientConfiguration gpgsConfig = new PlayGamesClientConfiguration.Builder()
				.RequestServerAuthCode(false)
				.RequestIdToken()
				.Build();

				PlayGamesPlatform.InitializeInstance(gpgsConfig);

				//PlayGamesPlatform.DebugLogEnabled = true;

				PlayGamesPlatform.Activate();
			#endif

			staticRef.hasInitialized = true;
		}

		void OnEnable()
		{
			if(!hasInitialized) return;

			if(auth != null){
				auth.StateChanged += AuthStateChanged;
				//auth.IdTokenChanged +=
			} else {
				Analytics.LogError("Auth Enable", "Auth was null!");
			}
		}

		void OnDisable()
		{
			if(!hasInitialized) return;

			if(auth != null){
				auth.StateChanged -= AuthStateChanged;
				//auth.IdTokenChanged -=
			} else {
				Analytics.LogError("Auth Disable", "Auth was null!");
			}
		}

		#if facebook
			void FacebookInitComplete()
			{
				if (FB.IsInitialized) {
					// FB.ActivateApp() doesn't like being called when not on an android/iOS device
					#if !UNITY_EDITOR
						// Facebook is initialized send the activation signal event
						FB.ActivateApp ();
					#endif
				} else {
					Analytics.LogError ("Facebook SDK", "Failed to initialize the Facebook SDK!");
				}
			}

			void FacebookOnHideUnity(bool isGameVisible)
			{
				if (!isGameVisible) {
					timescalePreFacebook = Time.timeScale;
					Time.timeScale = 0f;
				} else {
					Time.timeScale = timescalePreFacebook;
				}
			}

			public static void FacebookLogin()
			{
				if (FB.IsInitialized) {
					List<string> permissions = new List<string> (){ "public_profile", "email", "user_friends" };
					FB.LogInWithReadPermissions (permissions, staticRef.FacebookAuthCallback);
				} else {
					Analytics.LogError ("Facebook SDK", "Attempted to login before initizlized!");
				}
			}

			private void FacebookAuthCallback(ILoginResult result)
			{
				if (FB.IsLoggedIn) {
					// AcessToken class will have the session details
					activeFacebookAccessToken = result.AccessToken;// AccessToken.CurrentAccessToken;

					// Use the facebook token to auth with firebase
					AuthLogin(AuthType.Facebook);
				} else {
					Analytics.LogError ("Facebook Auth", "User canceled facebook login!");
				}
			}
		#endif

		#if twitter
			public static void TwitterLogin()
			{
				if (Twitter.Session == null) {
					Twitter.LogIn (staticRef.TwitterAuthSuccessCallback, staticRef.TwitterAuthFailureCallback);
				} else {
					staticRef.TwitterAuthSuccessCallback (Twitter.Session);
				}
			}

			// Warning! Your twitter app must be whitelisted to request user emails
			// This is done via https://support.twitter.com/forms/platform (speak to us about this if needed)
			public static void RequestTwitterEmail()
			{
				if (Twitter.Session != null) {
					Twitter.RequestEmail (Twitter.Session, staticRef.TwitterEmailRequestComplete, staticRef.TwitterEmailRequestFailed);
				} else {
					// Requested an email when the user isn't even logged in?
					Analytics.LogError("Twitter Email Request", "Attempted to request twitter email when a twitter session doesn't exist!");
				}
			}

			private void TwitterEmailRequestComplete(string email)
			{
				// Warning! You must store this email across sessions as repeated requests for the twitter email are not recomended!
				OnTwitterEmailReceived(email);
			}

			private void TwitterEmailRequestFailed(ApiError error)
			{
				Analytics.LogError ("Twitter Email Request", "Error (" + error.code + "): " + error.message);
			}

			private void TwitterAuthSuccessCallback(TwitterSession session)
			{
				// Auth was successful we can now attempt auth login again
				AuthLogin (AuthType.Twitter);
			}

			private void TwitterAuthFailureCallback(ApiError error)
			{
				Analytics.LogError ("Twitter Auth", "Login failed (" + error.code + "): " + error.message);
			}
		#endif

		#if google
			public static void GoogleLogin()
			{
				Social.localUser.Authenticate((bool success) => {
					if(success){
						staticRef.GoogleAuthSuccessCallback();
					} else {
						Analytics.LogError("Google Login", "Authentication failed!");

						ProjectManager.Log("Make sure a build has been uploaded to the Play Store and the google account you're trying to use has been added as a tester!");
					}
				});
				return;
			}

			private void GoogleAuthSuccessCallback()
			{
				// Auth was successful we can now attempt to auth login again
				AuthLogin(AuthType.Google);
			}
		#endif

		void AuthStateChanged(object sender, System.EventArgs eventArgs)
		{
			if(auth != null){
				if (auth.CurrentUser != activeUser) {
					bool signedIn = activeUser != auth.CurrentUser && auth.CurrentUser != null;

					// If we're not signed in and activeUser is not null (aka was previously set) then the user just logged out
					if (!signedIn && activeUser != null){
						OnLogout ();
					} else if(signedIn){
						OnLogin ();
					}
				}
			} else {
				Analytics.LogError("AuthStateChanged", "Auth was null!");
			}
		}

		/// <summary>
		/// Returned and encrypted string of the input string using SHA512 encryption
		/// </summary>
		/// <returns>The SHA512 encrypted string</returns>
		/// <param name="inputString">String to be encrypted</param>
		private static string sha512EncryptString(string inputString)
		{
			SHA512 shaCrypto = new SHA512Managed ();
			byte[] stringByteArray = Encoding.UTF8.GetBytes (inputString);

			byte[] encryptedStringByteArray = shaCrypto.ComputeHash (stringByteArray);
			StringBuilder encryptedString = new StringBuilder ();

			foreach (byte curByte in encryptedStringByteArray) {
				encryptedString.Append (curByte.ToString ("x2"));
			}

			return encryptedString.ToString();
		}

		/// <summary>
		/// Register the specified email and password to firebase
		/// </summary>
		/// <returns><c>true</c>, if registration was successful, <c>false</c> otherwise</returns>
		/// <param name="email">Email address</param>
		/// <param name="password">Plain text password</param>
		public static void FirebaseRegister(string email, string password, bool autoSendEmailVerification = false)
		{
			if(auth != null){
				// Encrypt the password so we're not just storing it in plain text
				string encryptedPassword = sha512EncryptString (password);

				ProjectManager.Log("[Firebase Registration] " + email + ", " + encryptedPassword + ", " + autoSendEmailVerification);

				auth.CreateUserWithEmailAndPasswordAsync (email, encryptedPassword).ContinueWith (task => {
					if(task.IsCanceled){
						Analytics.LogError("Firebase Registration", "Registration was canceled!");
						return;
					}

					if(task.IsFaulted){
						// Firebase for Unity is pretty undocumented for doing more than simply adding the plugins into projects..
						// Error handling doesn't seem great either, as of building this there's no error enum or error codes
						// So we just have strings to work with if we want to do actions on specific errors happening
						foreach(Exception e in task.Exception.InnerExceptions)
						{
							Analytics.LogError("Firebase Registration", e.Message); // This string only includes the firebase error, no information about the exception type

							OnRegistrationFailed(ConvertToAuthError(e.Message));
						}
						return;
					}

					if(task.IsCompleted){
						// Firebase user has successfully been created
						OnRegistrationSuccessful();

						// Note: At this point it will automatically be logged into the account and the login callback will be called
						if(autoSendEmailVerification) FirebaseSendEmailVerification(task.Result);
					}
				});
			} else {
				Analytics.LogError("Firebase Registration", "Auth was null!");
			}
		}

		/// <summary>
		/// Changes the user password
		/// </summary>
		/// <returns><c>true</c>, if password was changed, <c>false</c> otherwise</returns>
		/// <param name="newPassword">New plain text password</param>
		public static void FirebaseChangePassword(string newPassword, FirebaseUser requestedUser = null)
		{
			if (requestedUser == null) requestedUser = activeUser;

			// Encrypt the password so we're not just storing it in plain text
			string encryptedNewPassword = sha512EncryptString (newPassword);

			ProjectManager.Log("[Firebase Password Change] " + encryptedNewPassword);

			if(requestedUser != null){
				requestedUser.UpdatePasswordAsync (encryptedNewPassword).ContinueWith (task => {
					if(task.IsCanceled){
						Analytics.LogError("Firebase Password Change", "Change password was canceled!");
						return;
					}

					if(task.IsFaulted){
						// Firebase for Unity is pretty undocumented for doing more than simply adding the plugins into projects..
						// Error handling doesn't seem great either, as of building this there's no error enum or error codes
						// So we just have strings to work with if we want to do actions on specific errors happening
						foreach(Exception e in task.Exception.InnerExceptions)
						{
							Analytics.LogError("Firebase Password Change", e.Message); // This string only includes the firebase error, no information about the exception type

							OnPasswordChangeFailed(ConvertToAuthError(e.Message));
						}
						return;
					}

					if(task.IsCompleted){
						// Firebase user password changed successfully
						OnPasswordChangeSuccessful();
					}
				});
			} else {
				Analytics.LogError("Firebase Password Change", "User was null!");
			}
		}

		/// <summary>
		/// Sends a verification email to the email the user registered with, you can call IsEmailVerified() to check if the user has verified their email
		/// </summary>
		/// <returns><c>true</c>, if firebase verification email was sent, <c>false</c> otherwise.</returns>
		/// <param name="requestedUser">Requested user.</param>
		public static void FirebaseSendEmailVerification(FirebaseUser requestedUser = null)
		{
			if (requestedUser == null) requestedUser = activeUser;

			if(requestedUser != null){
				ProjectManager.Log("[Firebase Email Verify Send] " + requestedUser.DisplayName);

				if(!string.IsNullOrEmpty(requestedUser.Email)){
					requestedUser.SendEmailVerificationAsync ().ContinueWith (task => {
						if(task.IsCanceled){
							Analytics.LogError("Firebase Email Verify Send", "Send email verification canceled!");
							return;
						}

						if(task.IsFaulted){
							// Firebase for Unity is pretty undocumented for doing more than simply adding the plugins into projects..
							// Error handling doesn't seem great either, as of building this there's no error enum or error codes
							// So we just have strings to work with if we want to do actions on specific errors happening
							foreach(Exception e in task.Exception.InnerExceptions)
							{
								Analytics.LogError("Firebase Email Verify Send", e.Message); // This string only includes the firebase error, no information about the exception type

								OnSendEmailVerificationFailed(ConvertToAuthError(e.Message));
							}
							return;
						}

						if(task.IsCompleted){
							// Firebase email verification sent successfully

							OnSendEmailVerificationSuccessful();
						}
					});
				} else {
					Analytics.LogError("Firebase Email Verify Send", "User did not have an email address");

					#if twitter
						ProjectManager.Log("If this is a twitter user make sure the twitter app has permission to get email addresses!");
					#endif
				}
			} else {
				Analytics.LogError("Firebase Email Verify Send", "User was null!");
			}
		}

		/// <summary>
		/// Deletes the logged in user from the firebase user list
		/// </summary>
		/// <returns><c>true</c>, if firebase user was deleted, <c>false</c> otherwise.</returns>
		public static void FirebaseDeleteUser(FirebaseUser requestedUser = null)
		{
			if (requestedUser == null) requestedUser = activeUser;

			if(requestedUser != null){
				ProjectManager.Log("[Firebase User Delete] " + requestedUser.DisplayName);

				requestedUser.DeleteAsync ().ContinueWith (task => {
					if(task.IsCanceled){
						Analytics.LogError("Firebase User Delete", "Delete user canceled!");
						return;
					}

					if(task.IsFaulted){
						// Firebase for Unity is pretty undocumented for doing more than simply adding the plugins into projects..
						// Error handling doesn't seem great either, as of building this there's no error enum or error codes
						// So we just have strings to work with if we want to do actions on specific errors happening
						foreach(Exception e in task.Exception.InnerExceptions)
						{
							Analytics.LogError("Firebase User Delete", e.Message); // This string only includes the firebase error, no information about the exception type

							OnUserDeleteFailed(ConvertToAuthError(e.Message));
						}
						return;
					}

					if(task.IsCompleted){
						// Firebase account successfully deleted
						staticRef.OnLogout();

						OnUserDeleteSuccessful();
					}
				});
			} else {
				Analytics.LogError("Firebase User Delete", "User was null!");
			}
		}

		/// <summary>
		/// Login with the specified email and password
		/// </summary>
		/// <returns><c>true</c>, if login was successful, <c>false</c> otherwise</returns>
		/// <param name="email">User email</param>
		/// <param name="password">Plain text password</param>
		public static void FirebaseLogin(string email, string password)
		{
			// The stored password is encrypted so we need to encrypt the input password to match the string
			string encryptedPassword = sha512EncryptString(password);

			if(auth != null){
				ProjectManager.Log("[Firebase Login] " + email + ", " + encryptedPassword);

				auth.SignInWithEmailAndPasswordAsync (email, encryptedPassword).ContinueWith (task => {
					if (task.IsCanceled) {
						Analytics.LogError ("Firebase Login", "Login was canceled!");
						return;
					}

					if (task.IsFaulted) {
						// Firebase for Unity is pretty undocumented for doing more than simply adding the plugins into projects..
						// Error handling doesn't seem great either, as of building this there's no error enum or error codes
						// So we just have strings to work with if we want to do actions on specific errors happening
						foreach(Exception e in task.Exception.InnerExceptions)
						{
							Analytics.LogError("Firebase Login", e.Message); // This string only includes the firebase error, no information about the exception type

							OnLoginFailed(ConvertToAuthError(e.Message));
						}
						return;
					}

					if(task.IsCompleted){
						// Firebase login was successful
						staticRef.OnLogin();
					}
				});
			} else {
				Analytics.LogError("Firebase Login", "Auth was null!");
			}
		}

		/// <summary>
		/// Logout from the current logged in user
		/// </summary>
		public static void Logout()
		{
			ProjectManager.Log("[Firebase Logout] " + auth.CurrentUser.DisplayName);

			auth.SignOut ();

			staticRef.OnLogout ();
		}

		/// <summary>
		/// Unlinks the active auth method from the firebase account
		/// </summary>
		/// <returns><c>true</c>, if auth was unlinked, <c>false</c> otherwise.</returns>
		public static void UnLinkAuth(FirebaseUser requestedUser = null)
		{
			if (requestedUser == null) requestedUser = activeUser;

			string linkedAuth = "";

			if(auth != null){
				ProjectManager.Log("[Firebase Unlink Auth] " + requestedUser.DisplayName);

				auth.FetchProvidersForEmailAsync (GetEmail (requestedUser)).ContinueWith (task => {
					if(task.IsCanceled){
						Analytics.LogError("Firebase Get Provider", "Fetch providers for email canceled!");
						return;
					}

					if(task.IsFaulted){
						// Firebase for Unity is pretty undocumented for doing more than simply adding the plugins into projects..
						// Error handling doesn't seem great either, as of building this there's no error enum or error codes
						// So we just have strings to work with if we want to do actions on specific errors happening
						foreach(Exception e in task.Exception.InnerExceptions)
						{
							Analytics.LogError("Firebase Get Provider", e.Message); // This string only includes the firebase error, no information about the exception type

							OnGetProviderFailed(ConvertToAuthError(e.Message));
						}
						return;
					}

					if(task.IsCompleted){
						linkedAuth = task.Result.ToString();

						requestedUser.UnlinkAsync (linkedAuth).ContinueWith (unlinkTask => {
							if(unlinkTask.IsCanceled){
								Analytics.LogError("Firebase Unlink Auth", "UnLinkAuth was canceled!");
								return;
							}

							if(unlinkTask.IsFaulted){
								// Firebase for Unity is pretty undocumented for doing more than simply adding the plugins into projects..
								// Error handling doesn't seem great either, as of building this there's no error enum or error codes
								// So we just have strings to work with if we want to do actions on specific errors happening
								foreach(Exception e in unlinkTask.Exception.InnerExceptions)
								{
									Analytics.LogError("Firebase Unlink Auth", e.Message); // This string only includes the firebase error, no information about the exception type

									OnUnlinkAuthFailed(ConvertToAuthError(e.Message));
								}
								return;
							}

							if(unlinkTask.IsCompleted){
								// The auth method is now unlinked from the firebase user reference
								OnUnlinkAuthSuccessful();
							}
						});
					}
				});
			} else {
				Analytics.LogError("Firebase Unlink Auth", "Auth was null!");
			}
		}

		/// <summary>
		/// Login using an auth method. Note: Only 1 auth type can be linked at a time! You'll need to call UnLinkAuth if you want to UnLink an auth type
		/// </summary>
		/// <returns><c>true</c>, if login was successful, <c>false</c> otherwise.</returns>
		/// <param name="type">Auth type</param>
		public static void AuthLogin (AuthType type)
		{
			Credential credential = default(Credential); // For some reason if this is null instead of default Unity will just crash (even though the default seems to be null anyway ??)

			if(auth != null){
				ProjectManager.Log ("[Firebase AuthLogin] " + type.ToString ());

				switch (type) {
					case AuthType.Google:
						#if google
							if(Social.localUser == null || !Social.localUser.authenticated){
								GoogleLogin();
								return;
							} else {
								credential = GoogleAuthProvider.GetCredential (PlayGamesPlatform.Instance.GetIdToken(), PlayGamesPlatform.Instance.GetServerAuthCode());
							}
						#else
							Debug.LogError("Google auth login called but the Google Play Games Services plugin is now active in this project!");
						#endif
						break;

					case AuthType.Facebook:
						#if facebook
							if (!FB.IsLoggedIn || staticRef.activeFacebookAccessToken == null) {
								FacebookLogin (); // We need to wait for the facebook login callback before trying to auth again
								return;
							} else {			
								credential = FacebookAuthProvider.GetCredential (staticRef.activeFacebookAccessToken.TokenString);
							}
						#else
							Debug.LogError("Facebook auth login called but the facebook plugin in not active in this project!");
						#endif
						break;

					case AuthType.Twitter:
						#if twitter
							if(Twitter.Session == null || Twitter.Session.authToken == null){
								TwitterLogin(); // We need to wait for the twitter login callback before trying to auth again
								return;
							} else {
								credential = TwitterAuthProvider.GetCredential (Twitter.Session.authToken.token, Twitter.Session.authToken.secret);
							}
						#else
							Debug.LogError ("Twitter auth login called but the twitter plugin is not active in this project!");
						#endif
						break;
				}

				if (credential != null) {
					auth.SignInWithCredentialAsync (credential).ContinueWith (task => {
						if (task.IsCanceled) {
							Analytics.LogError ("Firebase AuthLogin " + type.ToString (), "AuthLogin was canceled!");
							return;
						}

						if (task.IsFaulted) {
							// Firebase for Unity is pretty undocumented for doing more than simply adding the plugins into projects..
							// Error handling doesn't seem great either, as of building this there's no error enum or error codes
							// So we just have strings to work with if we want to do actions on specific errors happening
							foreach(Exception e in task.Exception.InnerExceptions)
							{
								Analytics.LogError("Firebase AuthLogin", e.Message); // This string only includes the firebase error, no information about the exception type

								OnLoginFailed(ConvertToAuthError(e.Message));
							}
							return;
						}

						// Auth login was successful
						if(task.IsCompleted){
							staticRef.OnLogin ();

							// Attempt to link these auth'd credentials with firebase
							/*task.Result.LinkWithCredentialAsync (credential).ContinueWith (linkTask => {
								if (linkTask.IsCanceled) {
									Analytics.LogError ("Firebase Credential Link " + type.ToString (), "Link with credentials was canceled!");
									return;
								}

								if (linkTask.IsFaulted) {
									Analytics.LogError ("Firebase Credential Link " + type.ToString (), "Error: " + task.Exception);
									return;
								}

								// Link with credentials was a success, firebase will now keep track of this auth across games >:0
							});*/
						}
					});
				}
			} else {
				Analytics.LogError("Firebase AuthLogin", "Auth was null!");
			}
		}

		private void OnLogout()
		{
			// Update the activeUser to match the actual authed user (null if logged out)
			if(auth != null){
				activeUser = auth.CurrentUser;

				Analytics.LogEvent ("Logout"); // No built in event for logout
				Analytics.SetUserProperty ("logged_in", "false"); // We know that if this property is set the user has logged in before

				OnUserLogout ();
			} else {
				Analytics.LogError("Firebase Logout", "Auth was null!");
			}
		}

		private void OnLogin()
		{
			if(auth != null){
				#if analytics
					Analytics.LogEvent (FirebaseAnalytics.EventLogin);
				#endif

				Analytics.SetUserProperty ("logged_in", "true"); // We know that if this property isn't set the user has never logged in

				// Update the activeUser to match the actual authed user (null if logged out)
				activeUser = auth.CurrentUser;

				// Download the photo for this user so it's available if we request it
				GetPhotoTexture ();

				OnUserLogin ();
			} else {
				Analytics.LogError("Firebase Login", "Auth was null!");
			}
		}

		/// <summary>
		/// Check if we're logged in as a certain user or just check if logged in at all
		/// </summary>
		/// <returns><c>true</c> if is logged in the specified requestedUser (or just logged in at all if requestedUser not set) otherwise, <c>false</c>.</returns>
		/// <param name="requestedUser">User you want to check is signed in as (unset or null to just check if user is logged in at all)</param>
		public static bool IsLoggedIn (FirebaseUser requestedUser = null)
		{
			if (requestedUser == null) requestedUser = activeUser;

			if(auth != null && requestedUser != null){
				ProjectManager.Log("[Auth IsLoggedIn] " + requestedUser.DisplayName);

				return (auth.CurrentUser != null && auth.CurrentUser == requestedUser);
			} else {
				Analytics.LogError("IsLoggedIn", "Auth or user was null!");
				return false;
			}
		}

		public static string GetDisplayName(FirebaseUser requestedUser = null)
		{
			if (requestedUser == null) requestedUser = activeUser;

			if(requestedUser != null){
				ProjectManager.Log("[Auth GetDisplayName] " + requestedUser.DisplayName);

				return requestedUser.DisplayName;
			} else {
				Analytics.LogError("GetDisplayName", "User was null!");
				return string.Empty;
			}
		}

		public static string GetEmail(FirebaseUser requestedUser = null)
		{
			if (requestedUser == null) requestedUser = activeUser;

			if(requestedUser != null){
				ProjectManager.Log("[Auth GetEmail] " + requestedUser.Email);

				return requestedUser.Email;
			} else {
				Analytics.LogError("GetEmail", "User was null!");
				return string.Empty;
			}
		}

		public static bool IsEmailVerified(FirebaseUser requestedUser = null)
		{
			if (requestedUser == null) requestedUser = activeUser;

			if(requestedUser != null){
				ProjectManager.Log("[Auth IsEmailVerified] " + requestedUser.IsEmailVerified);

				return requestedUser.IsEmailVerified;
			} else {
				Analytics.LogError("IsEmailVerified", "User was null!");
				return false;
			}
		}

		public static string GetPhotoURL(FirebaseUser requestedUser = null)
		{
			if (requestedUser == null) requestedUser = activeUser;

			if(requestedUser != null){
				// Warning! PhotoUrl is null when the user doesn't have a photo which will cause an exception when referring to subclasses
				ProjectManager.Log("[Auth GetPhotoURL] " + (requestedUser.PhotoUrl != null ? requestedUser.PhotoUrl.AbsoluteUri : string.Empty));

				return (requestedUser.PhotoUrl != null ? requestedUser.PhotoUrl.AbsoluteUri : string.Empty);
			} else {
				Analytics.LogError("GetPhotoURL", "User was null!");
				return string.Empty;
			}
		}

		public static string GetUserID(FirebaseUser requestedUser = null)
		{
			if (requestedUser == null) requestedUser = activeUser;

			if(requestedUser != null){
				ProjectManager.Log("[Auth GetUserID] " + requestedUser.UserId);

				return requestedUser.UserId;
			} else {
				Analytics.LogError("GetUserID", "User was null!");
				return string.Empty;
			}
		}

		/// <summary>
		/// Returns the photo texture OR requests it if it's not yet downloaded
		/// </summary>
		/// <returns>The photo texture if downloaded</returns>
		/// <param name="forceRedownload">If set to <c>true</c> active downloads will be canceled and the download will start again, if the image is already downloaded this will also force it to be downloaded again</param>
		public static Texture2D GetPhotoTexture(bool forceRedownload = false)
		{
			ProjectManager.Log("[Firebase GetPhotoTexture] " + forceRedownload);

			if (forceRedownload) {
				// Even if the photo is downloading or downloaded this will cancel the request and redownload it
				isDownloadingPhoto = false;
				isPhotoDownloaded = false;
			}

			if (!isDownloadingPhoto) {
				if (isPhotoDownloaded) {
					return photoTexture;
				} else {
					// The photo isn't already downloading and isn't downloaded yet so start the request
					staticRef.StartPhotoDownload ();
				}
			} else {
				// The photo is downloading so we just need to wait..
			}

			return null; // The photo has not downloaded yet
		}

		private void StartPhotoDownload()
		{
			ProjectManager.Log("[Firebase StartPhotoDownload]");

			StartCoroutine (DownloadPhoto ());
		}

		private IEnumerator DownloadPhoto ()
		{
			string photoURL = GetPhotoURL ();

			ProjectManager.Log ("[Firebase DownloadPhoto] " + photoURL);

			if (!string.IsNullOrEmpty (photoURL)) {
				// Start the photo web request
				WWW photoRequest = new WWW (GetPhotoURL ()); // We're fine to assume the URL from social networks are safe

				isDownloadingPhoto = true;
				isPhotoDownloaded = false;

				// Wait for the photo download request to finish (if isDownloadingPhoto is set to false during the download it will force stop the coroutine)
				while (!photoRequest.isDone && string.IsNullOrEmpty (photoRequest.error) && isDownloadingPhoto) {
					yield return null;
				}

				// If there was no issues with the photo download then set the photoTexture as the downloaded texture
				if (photoRequest.isDone && string.IsNullOrEmpty (photoRequest.error) && isDownloadingPhoto) {
					ProjectManager.Log("Photo download successful!");

					photoTexture = photoRequest.texture;

					OnPhotoReady ();

					isPhotoDownloaded = true;
					isDownloadingPhoto = false;
				} else {
					if(!photoRequest.isDone) 
						ProjectManager.Log("Photo download finished before it was done!");

					if(!string.IsNullOrEmpty(photoRequest.error)) 
						ProjectManager.Log("Photo download error: " + photoRequest.error);

					if(!isDownloadingPhoto) 
						ProjectManager.Log("Cancelled at user request!");

					isPhotoDownloaded = false;
					isDownloadingPhoto = false;
				}

				// Cleanup the web request or stop the request if the coroutine was force ended with isDownloadingPhoto being set to false early
				photoRequest.Dispose ();
			}
		}
	#else
		public static void Initialize(){}

		public static void FacebookLogin(){}
		public static void TwitterLogin(){}
		public static void RequestTwitterEmail(){}
		public static void GoogleLogin(){}
		public static void FirebaseRegister<T>(T email, T password, bool AutoLogin = false, bool AutoSendEmailVerification = false){}
		public static void FirebaseChangePassword<T>(T newPassword){}
		public static void FirebaseSendEmailVerification<T>(T requestedUser){}
		public static void FirebaseSendEmailVerification(){}
		public static void FirebaseDeleteUser(){}
		public static void FirebaseLogin<T>(T email, T password){}
		public static void Logout(){}
		public static void UnLinkAuth(){}
		public static void AuthLogin<T>(T type){}

		public static bool IsLoggedIn() { return false; }
		public static string GetDisplayName() { return ""; }
		public static string GetEmail() { return ""; }
		public static bool IsEmailVerified() { return false; }
		public static string GetPhotoURL() { return ""; }
		public static string GetUserID() { return ""; }

		public static Texture2D GetPhotoTexture(bool forceRedownload = false){ return null; }
	#endif
}
