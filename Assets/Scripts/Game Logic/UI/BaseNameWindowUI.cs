using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BaseNameWindowUI : UIPanel
{
	public TMP_InputField text;
	public TMP_Text baseNameText;

	void Awake()
	{
		GameRegistry.INST.baseNameUI = this;
		OnShow += () =>
		{
			EventManager.InvokeEvent("nameWindowOpen");
		};
		OnHide += () =>
		{
			baseNameText.text = text.text;
			EventManager.InvokeEvent("nameWindowClose");
		};
	}
}