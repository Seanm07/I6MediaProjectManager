using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleMenuButtonHandler : MonoBehaviour {

	public enum ButtonType {
		ShowBottomSmartBanner,
		HideBottomSmartBanner,
		LogHelloEvent,
		GoogleAuth,
		TwitterAuth,
		FacebookAuth,
		EmailRegister,
		EmailSendVerification,
		EmailAuth,
		UnAuth,
		RefreshIAS,
		SendInvite,
		ShowInterstitial
	}

	public enum EventType {
		OnClick,
		OnPress,
		OnRelease, 
		OnDoubleClick,
		OnUpdateWhilePressed
	}

	public EventType ButtonEventType = EventType.OnClick;
	public ButtonType ButtonActionType;
	public bool DoHapticFeedback = true;

	private bool IsPressDown = false;

	// Make sure variables don't get stuck when leaving a screen
	void OnDisable()
	{
		IsPressDown = false;
	}

	public void OnControllerSelect()
	{
		CallEvent ();
	}

	void OnClick()
	{
		if (ButtonEventType == EventType.OnClick)
			CallEvent ();
	}

	void OnPress(bool IsDown)
	{
		IsPressDown = IsDown;

		if (ButtonEventType == EventType.OnPress && IsDown)
			CallEvent ();
	}

	void OnDoubleClick()
	{
		if (ButtonEventType == EventType.OnDoubleClick)
			CallEvent ();
	}

	void Update()
	{
		if (ButtonEventType == EventType.OnUpdateWhilePressed && IsPressDown)
			CallEvent ();
	}

	public void CallEvent()
	{
		bool ValidButton = true;

		switch (ButtonActionType) {
			case ButtonType.ShowBottomSmartBanner:
				AdMob_Manager.Instance.LoadBanner (GoogleMobileAds.Api.AdSize.SmartBanner, GoogleMobileAds.Api.AdPosition.Bottom, true);
				break;

			case ButtonType.HideBottomSmartBanner:
				AdMob_Manager.Instance.HideBanner (false);
				break;

			case ButtonType.ShowInterstitial:
				AdMob_Manager.Instance.ShowInterstitial();
				break;

			case ButtonType.LogHelloEvent:
				Analytics.LogEvent ("Hello", "World", "This is a test");
				Analytics.LogEvent ("Hello", "Office", "This is a test");
				Analytics.LogEvent ("Goodbye", "World");
				Analytics.LogEvent ("Hello", "World", "How's it going?");
				Analytics.LogEvent ("Hello", "World", 4);
				break;

			case ButtonType.GoogleAuth:
				Auth.AuthLogin (Auth.AuthType.Google);
				break;

			case ButtonType.TwitterAuth:
				Auth.AuthLogin (Auth.AuthType.Twitter);
				break;

			case ButtonType.FacebookAuth:
				Auth.AuthLogin (Auth.AuthType.Facebook);
				break;

			case ButtonType.EmailRegister:
				Auth.FirebaseRegister ("sean@i6.com", "123456", false);
				break;

			case ButtonType.EmailSendVerification:
				Auth.FirebaseSendEmailVerification ();
				break;

			case ButtonType.EmailAuth:
				Auth.FirebaseLogin ("sean@i6.com", "123456");
				break;

			case ButtonType.UnAuth:
				Auth.UnLinkAuth ();
				break;

			case ButtonType.RefreshIAS:
				// N/A
				break;

			case ButtonType.SendInvite:
				Invites.SendInvite ("Title here", "Message here", "Call to action", "http://www.website.com");
				break;

			default:
				ValidButton = false;
				break;
		}

		if (ValidButton && DoHapticFeedback) {
			//JarLoader.DoHapticFeedback(1);
		}
	}

}
