using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BaseNameWindowUI : UIPanel
{
	public TMP_InputField text;
	public TMP_Text baseNameText;
	public InteractionUI interactionUI;

	void Awake()
	{
		var cam = Camera.main.GetComponent<CameraController>();
		OnShow += () =>
		{
			cam.enabled = false;
			interactionUI.interactionPanel.HidePanel();
			interactionUI.enabled = false;
		};
		OnHide += () =>
		{
			baseNameText.text = text.text;
			interactionUI.enabled = true;
			cam.enabled = true;
		};
	}
}