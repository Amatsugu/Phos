using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

using UnityEngine;

public class GameUI : UIHover
{
	private UIInfoPanel _infoPanel;
	private UIBuildPanel _buildPanel;
	private UIActionsPanel _actionsPanel;
	private UICategoryPanel _categoryPanel;
	private UISelectionPanel _selectionPanel;

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
		_infoPanel = GetComponentInChildren<UIInfoPanel>(true);
		_infoPanel.gameObject.SetActive(true);
		_buildPanel = GetComponentInChildren<UIBuildPanel>(true);
		_buildPanel.gameObject.SetActive(true);
		_actionsPanel = GetComponentInChildren<UIActionsPanel>(true);
		_actionsPanel.gameObject.SetActive(true);
		_categoryPanel = GetComponentInChildren<UICategoryPanel>(true);
		_categoryPanel.gameObject.SetActive(true);
		_selectionPanel = GetComponentInChildren<UISelectionPanel>(true);
		_selectionPanel.gameObject.SetActive(true);

		_categoryPanel.OnButtonClicked += CategorySelected;
		_buildPanel.OnHide += OnBuildPanelClosed;

		_buildPanel.infoPanel = _infoPanel;

		GameEvents.OnGameReady += Init;
	}

	private void Init()
	{
		state = UIState.PlaceBuilding;
	}

	private void CategorySelected(BuildingCategory category)
	{
		state = UIState.PlaceBuilding;
		_buildPanel.Show(category);
	}

	private void OnBuildPanelClosed()
	{
		state = UIState.Idle;
	}

	protected override void Update()
	{
		base.Update();
		if (!isHovered)
			_buildPanel.UpdateState();
		if (state == UIState.Idle)
			_selectionPanel.UpdateState();
	}


	
}