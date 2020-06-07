using Steamworks;
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
		Deconstruct
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
		_categoryPanel.OnDeconstructClick += EnterDeconstructMode;

		GameEvents.OnGameReady += Init;
		GameEvents.OnHQPlaced += OnHQPlaced;
		_categoryPanel.SetInteractable(false);
	}

	protected override void Start()
	{
		base.Start();
	}

	private void OnHQPlaced()
	{
		state = UIState.Idle;
		_categoryPanel.SetInteractable(true);
	}

	private void Init()
	{
		state = UIState.HQPlacement;
		_selectionPanel.OnHide += OnSelectionClosed;
		_buildPanel.OnHide += OnBuildPanelClosed;
	}

	private void CategorySelected(BuildingCategory category)
	{
		_selectionPanel.Hide();
		state = UIState.PlaceBuilding;
		_buildPanel.Show(category);
	}

	private void EnterDeconstructMode()
	{
		_buildPanel.Hide();
		_categoryPanel.DeselectAll();
		state = UIState.Deconstruct;
		_buildPanel.state = UIBuildPanel.BuildState.Deconstruct;
	}

	private void OnBuildPanelClosed()
	{
		if (state == UIState.HQPlacement)
			return;
		state = UIState.Idle;
		_categoryPanel.DeselectAll();
	}

	private void OnSelectionClosed()
	{
		if (state == UIState.HQPlacement)
			return;
		state = UIState.Idle;
		_actionsPanel.Hide();
	}

	protected override void Update()
	{
		base.Update();
		if (isHovered)
		{
			_buildPanel.indicatorManager.HideAllIndicators();
			_buildPanel.indicatorManager.UnSetAllIndicators();
			return;
		}
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
			case UIState.Deconstruct:
				_buildPanel.UpdateState();
				break;
		}
		
	}


	
}