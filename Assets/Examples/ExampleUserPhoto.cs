using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleUserPhoto : MonoBehaviour {

	private UITexture selfTexture;

	void Awake()
	{
		selfTexture = GetComponent<UITexture> ();
	}

	void OnEnable()
	{
		Auth.OnPhotoReady += OnPhotoReady;
	}

	void OnDisable()
	{
		Auth.OnPhotoReady -= OnPhotoReady;
	}

	void OnPhotoReady()
	{
		selfTexture.mainTexture = Auth.GetPhotoTexture ();
	}

}
