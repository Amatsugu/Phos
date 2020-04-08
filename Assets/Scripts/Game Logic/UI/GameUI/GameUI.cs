using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : UIHover
{
	private UIInfoPanel _infoPanel;
	private UIBuildPanel _buildPanel;
	private UIActionsPanel _actionsPanel;
	private UICategoryPanel _categoryPanel;

	private UIState state;

	private enum UIState
	{
		Disabled,
		Idle,
		PlaceBuilding,
		PlaceUnit,
		BuildingsSelected,
		UnitsSelected,
	}

	protected override void Awake()
	{
		base.Awake();
		_infoPanel = GetComponentInChildren<UIInfoPanel>();
		_buildPanel = GetComponentInChildren<UIBuildPanel>();
		_actionsPanel = GetComponentInChildren<UIActionsPanel>();
		_categoryPanel = GetComponentInChildren<UICategoryPanel>();


		_categoryPanel.OnButtonClicked += CategorySelected;
		_buildPanel.OnHide += OnBuildPanelClosed;

		GameEvents.OnGameReady += Init;
	}

	private void Init()
	{
		state = UIState.PlaceBuilding;
	}

	private void CategorySelected(BuildingCategory category)
	{
		state = UIState.PlaceBuilding;
		_buildPanel.Show();
	}

	private void OnBuildPanelClosed()
	{
		state = UIState.Idle;
	}
}
