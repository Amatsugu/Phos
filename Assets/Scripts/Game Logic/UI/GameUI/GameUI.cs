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
		HQPlacement,
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


		_selectionPanel.actionsPanel = _actionsPanel;
		_buildPanel.infoPanel = _infoPanel;

		_categoryPanel.OnButtonClicked += CategorySelected;

		GameEvents.OnGameReady += Init;
		GameEvents.OnHQPlaced += OnHQPlaced;
	}

	private void OnHQPlaced()
	{
		state = UIState.Idle;
	}

	private void Init()
	{
		state = UIState.HQPlacement;
		_selectionPanel.OnHide += OnSelectionClosed;
		_buildPanel.OnHide += OnBuildPanelClosed;
	}

	private void CategorySelected(BuildingCategory category)
	{
		state = UIState.PlaceBuilding;
		_buildPanel.Show(category);
	}

	private void OnBuildPanelClosed()
	{
		if (state == UIState.HQPlacement)
			return;
		state = UIState.Idle;
	}

	private void OnSelectionClosed()
	{
		if (state == UIState.HQPlacement)
			return;
		state = UIState.Idle;
		Debug.Log("Selection Closed");
		_actionsPanel.Hide();
	}

	protected override void Update()
	{
		base.Update();
		if (isHovered)
			return;
		switch(state)
		{
			case UIState.Idle:
				_selectionPanel.UpdateState();
				_actionsPanel.UpdateState();
				break;
			case UIState.PlaceBuilding:
				_buildPanel.UpdateState();
				break;
			case UIState.HQPlacement:
				_buildPanel.UpdateState();
				break;
		}
		
	}


	
}