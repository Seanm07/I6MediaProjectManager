using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleUserEmail : MonoBehaviour {

	private UILabel selfLabel;

	void Awake()
	{
		selfLabel = GetComponent<UILabel> ();
	}

	void OnEnable()
	{
		Auth.OnUserLogin += OnUserChange;
		Auth.OnUserLogout += OnUserChange;
	}

	void OnDisable()
	{
		Auth.OnUserLogin -= OnUserChange;
		Auth.OnUserLogout -= OnUserChange;
	}

	void OnUserChange()
	{
		selfLabel.text = Auth.GetEmail ();
	}
}
