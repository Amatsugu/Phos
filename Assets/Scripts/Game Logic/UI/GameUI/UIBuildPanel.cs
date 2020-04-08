using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBuildPanel : UIPanel
{
	public UIUnitIcon iconPrefab;

	public RectTransform contentArea;

	private UIUnitIcon[] _icons;


	protected override void Awake()
	{
		base.Awake();
		_icons = new UIUnitIcon[12];
		OnShow += OnPanelShow;
	}

	private void OnPanelShow()
	{

	}

}
