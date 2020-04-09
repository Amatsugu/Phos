using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICategoryPanel : UITabPanel
{
	public event Action<BuildingCategory> OnButtonClicked;

	protected override void OnTabSelected(int tab)
	{
		base.OnTabSelected(tab);
		OnButtonClicked?.Invoke((BuildingCategory)(tab+1));
	}

	public override void ClearAllEvents()
	{
		base.ClearAllEvents();
		ClearButtonClickedEvent();
	}

	public void ClearButtonClickedEvent() => OnButtonClicked = null;

}
