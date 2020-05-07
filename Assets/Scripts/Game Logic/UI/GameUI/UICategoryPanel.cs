using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICategoryPanel : UITabPanel
{
	public event Action<BuildingCategory> OnButtonClicked;
	public event Action OnDeconstructClick;
	public Button deconstructButton;

	protected override void Awake()
	{
		base.Awake();
		deconstructButton.onClick.AddListener(() => OnDeconstructClick?.Invoke());
	}

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

	public void SetInteractable(bool isInteractable)
	{
		for (int i = 0; i < tabs.Length; i++)
		{
			tabs[i].interactable = isInteractable;
		}
	}

	public void ClearButtonClickedEvent() => OnButtonClicked = null;

}
