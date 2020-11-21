using Amatsugu.Phos;

using Steamworks;

using System;
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
	private UIBuildQueuePanel _buildQueuePanel;

	private UIState _state;
	private UIState _prevState = UIState.HQPlacement;


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
		_buildQueuePanel = GetComponentInChildren<UIBuildQueuePanel>(true);
		_buildQueuePanel.gameObject.SetActive(true);

		_selectionPanel.actionsPanel = _actionsPanel;
		_buildPanel.infoPanel = _infoPanel;
		_buildPanel.buildQueueUI = _buildQueuePanel;

		_categoryPanel.OnButtonClicked += CategorySelected;
		_categoryPanel.OnDeconstructClick += EnterDeconstructMode;

		GameEvents.OnGameReady += Init;
		GameEvents.OnHQPlaced += OnHQPlaced;
		_categoryPanel.SetInteractable(false);

		GameEvents.OnDevConsoleOpen += OnDevOpen;
		GameEvents.OnDevConsoleClose += OnDevClose;
	}

	private void OnDevOpen()
	{
		_prevState = _state;
		_state = UIState.Disabled;
	}

	private void OnDevClose()
	{
		_state = _prevState;
	}

	private void OnMapLoad()
	{
		_selectionPanel.Hide();
		_buildPanel.Hide();
	}

	protected override void Start()
	{
		base.Start();
	}

	private void OnHQPlaced()
	{
		_state = UIState.Idle;
		_prevState = _state;
		_categoryPanel.SetInteractable(true);
	}

	private void Init()
	{
		_state = UIState.HQPlacement;
		_selectionPanel.OnHide += OnSelectionClosed;
		_buildPanel.OnHide += OnBuildPanelClosed;
		GameEvents.OnMapLoaded += OnMapLoad;
	}

	private void CategorySelected(BuildingCategory category)
	{
		_selectionPanel.Hide();
		_prevState = _state = UIState.PlaceBuilding;
		_buildPanel.Show(category);
	}

	private void EnterDeconstructMode()
	{
		_buildPanel.Hide();
		_categoryPanel.DeselectAll();
		_prevState = _state = UIState.Deconstruct;
		_buildPanel.state = UIBuildPanel.BuildState.Deconstruct;
	}

	private void OnBuildPanelClosed()
	{
		if (_state == UIState.HQPlacement)
			return;
		_state = UIState.Idle;
		_categoryPanel.DeselectAll();
	}

	private void OnSelectionClosed()
	{
		if (_state == UIState.HQPlacement)
			return;
		_state = UIState.Idle;
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
		switch(_state)
		{
			case UIState.Idle:
				_selectionPanel.UpdateState();
				_actionsPanel.UpdateState();
				_buildPanel.UpdateState();
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

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GameEvents.OnDevConsoleClose -= OnDevClose;
		GameEvents.OnDevConsoleOpen -= OnDevOpen;
	}


}