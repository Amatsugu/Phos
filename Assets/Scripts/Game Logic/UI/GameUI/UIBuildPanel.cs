﻿using Amatsugu.Phos;
using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;

using System;
using System.Collections.Generic;
using System.Linq;

using TMPro;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;

using UnityEngine;
using UnityEngine.Profiling;

public class UIBuildPanel : UITabPanel
{
	public UIUnitIcon iconPrefab;

	[HideInInspector]
	public UIInfoPanel infoPanel;
	[HideInInspector]
	public UIBuildQueuePanel buildQueueUI;
	[HideInInspector]
	public UITooltip tooltipUI;

	public TMP_Text floatingText;
	public UIInfoBanner banner;
	public float inidcatorOffset = .5f;
	public HQTileEntity hQTile;
	public RectTransform contentArea;
	public GameObject nothingUnlockedText;
	public float showPowerRange = 20;

	[Header("Indicators")]
	public GameObject poweredTileIndicatorPrefab;
	public GameObject unpoweredTileIndicatorPrefab;
	public GameObject destructionIndicatorPrefab;

	public BuildState state;

	[HideInInspector]
	public IndicatorManager indicatorManager;

	private UIUnitIcon[] _icons;
	private BuildingTileEntity _selectedBuilding;
	private int _tier = 1;
	private BuildingCategory _lastCategory;
	private float _poweredTileRangeSq;
	private Camera _cam;
	private BuildingDatabase _buildingDatabase;
	private FactoryBuildingTile _selectedFactory;
	private UnitDatabase _unitDatabase;
	private BuildQueueSystem _buildQueue;
	private UnitFactorySystem _unitFactory;
	private int _rotation;

	public enum BuildState
	{
		Disabled = 0,
		Idle,
		HQPlacement,
		Placement,
		UnitConstruction,
		Deconstruct
	}

	protected override void Awake()
	{
		OnHide += () =>
		{
			if (state == BuildState.HQPlacement)
				return;
			HideIndicators();
			buildQueueUI.Hide();
			state = BuildState.Disabled;
		};
		_icons = new UIUnitIcon[8];
		GameEvents.OnGameReady += OnGameReady;
		_poweredTileRangeSq = showPowerRange * showPowerRange;
		_buildingDatabase = GameRegistry.BuildingDatabase;
		_unitDatabase = GameRegistry.UnitDatabase;

		GameRegistry.RegisterPrefabForInit(poweredTileIndicatorPrefab);
		GameRegistry.RegisterPrefabForInit(unpoweredTileIndicatorPrefab);
		GameRegistry.RegisterPrefabForInit(destructionIndicatorPrefab);

		indicatorManager = new IndicatorManager(World.DefaultGameObjectInjectionWorld.EntityManager, inidcatorOffset, floatingText);
		_buildQueue = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildQueueSystem>();
		_unitFactory = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<UnitFactorySystem>();



		Debug.Log("Init Build Panel");
		base.Awake();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		indicatorManager.Dispose();
	}

	protected override void Start()
	{
		base.Start();
		_cam = GameRegistry.Camera;
	}

	private void OnGameReady()
	{
		state = BuildState.HQPlacement;
		_selectedBuilding = hQTile;
		GameEvents.OnGameTick += OnTick;
		GameEvents.OnBuildingUnlocked += b =>
		{
			if (IsOpen)
				Show(_lastCategory);

			NotificationsUI.Notify(b.icon, "Building Unlocked", $"<b>{b.GetNameString()}</b>\n<size=-1>{b.description}</size>", 10);
		};
	}

	protected override void OnTabSelected(int tab)
	{
		base.OnTabSelected(tab);
		_tier = tab + 1;
		if (state == BuildState.UnitConstruction)
			Show(_selectedFactory);
		else
			Show(_lastCategory);
	}

	private void OnTick()
	{
		if (state == BuildState.Disabled || state == BuildState.HQPlacement)
			return;
		if (state == BuildState.UnitConstruction)
		{
			return;
		}
		var buildings = _buildingDatabase[_lastCategory];
		for (int i = 0, j = 0; i < _icons.Length; i++)
		{
			if (_icons[i] == null)
			{
				_icons[i] = Instantiate(iconPrefab, contentArea, false);
				_icons[i].OnBlur += () => tooltipUI.Hide();
			}
			if (j < buildings.Length)
			{
				if (buildings[j].info.tier != _tier)
					continue;
				if (!GameRegistry.IsBuildingUnlocked(buildings[j].id))
					continue;
				_icons[i].titleText.SetText(buildings[j].info.GetNameString());
				_icons[i].costText.SetText(buildings[j].info.GetCostString());
				_icons[i].icon.sprite = buildings[j].info.icon;
				var b = buildings[j];
				j++;
			}
		}
	}

	public void Show(FactoryBuildingTile factoryTile)
	{
		if (state == BuildState.HQPlacement) //This should never happen, but just in case
			return;
		if(state != BuildState.UnitConstruction)
		{
			base.OnTabSelected(0);
			_tier = 1;
		}
		buildQueueUI.Show(factoryTile);
		state = BuildState.UnitConstruction;
		_selectedFactory = factoryTile;
		var units = _selectedFactory.factoryInfo.unitsToBuild;
		bool hasIcons = false;
		for (int i = 0, j = 0; i < _icons.Length; i++)
		{
			if (_icons[i] == null)
			{
				_icons[i] = Instantiate(iconPrefab, contentArea, false);
				_icons[i].OnBlur += () => tooltipUI.Hide();
			}
			if (j < units.Length)
			{
				var unitId = units[j];
				var unit = _unitDatabase.unitEntites[unitId.id].info;
				if (unit.tier == _tier)
				{
					_icons[i].SetActive(false);
					_icons[i].ClearHoverEvents();
					_icons[i].ClearClickEvents();
					_icons[i].titleText.SetText(unit.GetNameString());
					_icons[i].costText.SetText(unit.GetCostString());
					_icons[i].icon.sprite = unit.icon;
					_icons[i].button.interactable = true;
					_icons[i].OnHover += () => tooltipUI.Show(unit);
					_icons[i].OnClick += () =>
					{
						_unitFactory.QueueUnit(unitId, factoryTile.Coords, Faction.Player, unit.buildTime);
					};
					_icons[i].SetActive(true);
					hasIcons = true;
				}
				else
					i--;
				j++;
			}
			else
				_icons[i].SetActive(false);
		}
		nothingUnlockedText.SetActive(!hasIcons);
		Show();
	}

	public void Show(BuildingCategory category)
	{
		if (state == BuildState.HQPlacement)
			return;
		buildQueueUI.Hide();
		if (state != BuildState.Idle && state != BuildState.Placement)
		{
			base.OnTabSelected(0);
			_tier = 1;
		}
		state = BuildState.Idle;
		_lastCategory = category;
		var buildings = _buildingDatabase[category];
		bool hasIcons = false;
		for (int i = 0, j = 0; i < _icons.Length; i++)
		{
			if (_icons[i] == null)
			{
				_icons[i] = Instantiate(iconPrefab, contentArea, false);
				_icons[i].OnBlur += () => tooltipUI.Hide();
			}
			if (j < buildings.Length)
			{
				if (buildings[j].info.tier == _tier && GameRegistry.IsBuildingUnlocked(buildings[j].id))
				{
					_icons[i].SetActive(false);
					_icons[i].ClearHoverEvents();
					_icons[i].ClearClickEvents();
					var b = buildings[j];
					_icons[i].titleText.SetText(b.info.GetNameString());
					_icons[i].costText.SetText(b.info.GetCostString());
					_icons[i].icon.sprite = b.info.icon;
					_icons[i].button.interactable = true;
					if (category == BuildingCategory.Tech)
					{
						if (GameRegistry.GameMap.HasTechBuilding(b.info as TechBuildingTileEntity))
							_icons[i].button.interactable = false;
					}
					_icons[i].OnHover += () => tooltipUI.Show(b.info);
					_icons[i].OnClick += () =>
					{
						state = BuildState.Placement;
						_rotation = 0;
						_selectedBuilding = b.info;
					};
					_icons[i].SetActive(true);
					hasIcons = true;
				}
				else
					i--;
				j++;
			}
			else
				_icons[i].SetActive(false);
		}
		nothingUnlockedText.SetActive(!hasIcons);
		Show();
	}

	public void UpdateState()
	{
		if (state == BuildState.Idle && !isHovered && Input.GetKeyDown(KeyCode.Mouse0))
			Hide();
		switch (state)
		{
			case BuildState.Disabled:
				InfoPanelLogic();
				break;

			case BuildState.Idle:
				InfoPanelLogic();
				ReadBackInput();
				break;

			case BuildState.HQPlacement:
				ValidatePlacement();
				break;

			case BuildState.Placement:
				ValidatePlacement();
				ReadBackInput();
				break;

			case BuildState.Deconstruct:
				DeconstructLogic();
				ReadBackInput();
				break;

			case BuildState.UnitConstruction:
				InfoPanelLogic();
				ReadBackInput();
				break;
		}
	}

	private void InfoPanelLogic()
	{
		infoPanel.Hide();
		tooltipUI.Hide();
		var (tile, _) = GetTileUnderCursor();
		if (tile is BuildingTile b)
		{
			tooltipUI.Show(b.GetDescriptionString().AppendLine(b.GetProductionString().ToString()), b.GetNameString());
			//infoPanel.ShowInfo(b);
		}
		else if (tile is ResourceTile r)
		{
			tooltipUI.Show(r.GetDescriptionString().AppendLine(r.GetProductionString().ToString()), r.GetNameString());
			//infoPanel.ShowInfo(r);
		}
		if (Input.GetKeyUp(KeyCode.Mouse0) && tile is FactoryBuildingTile f && f.IsBuilt)
			Show(f);
	}

	private void DeconstructLogic()
	{
		indicatorManager.UnSetAllIndicators();
		tooltipUI.Hide();

		var (selectedTile, _) = GetTileUnderCursor();
		if (selectedTile == null)
			return;
		if (selectedTile is not IDeconstructable deconstructable)
			return;
		Debug.Log(deconstructable.GetType().Name);
		if (selectedTile is BuildingTile b)
		{
			var footprint = b.buildingInfo.footprint.GetOccupiedTiles(selectedTile.Coords, b.Rotation);
			tooltipUI.Show(b.GetDescriptionString().AppendLine(b.GetProductionString().ToString()), b.GetNameString());
			foreach (var t in footprint)
			{
				indicatorManager.SetIndicator(GameRegistry.GameMap[t], destructionIndicatorPrefab);
			}
		}
		else
		{
			indicatorManager.SetIndicator(selectedTile, destructionIndicatorPrefab);
		}
		if (Input.GetKeyUp(KeyCode.Mouse0) && deconstructable.CanDeconstruct(Faction.Player))
		{
			Debug.Log($"Can Deconstruct: {deconstructable.CanDeconstruct(Faction.Player)}");
			var postUpdateCommands = GameRegistry.EntityManager.World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>().CreateCommandBuffer();
			var tiles = GameRegistry.GetTileInstanceBuffer();
			var prefabs = GameRegistry.EntityManager.GetBuffer<GenericPrefab>(GameRegistry.MapEntity);
			deconstructable.Deconstruct(prefabs, tiles, postUpdateCommands);
		}
	}

	private void ReadBackInput()
	{
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Mouse1))
		{
			switch (state)
			{
				case BuildState.Placement:
					ExitPlacementMode();
					break;

				case BuildState.Deconstruct:
					ExitDeconstructionMode();
					break;
				default:
					Hide();
					break;
			}
		}
	}

	public void EnterDeconstructionMode()
	{
		state = BuildState.Deconstruct;
		HideIndicators();
	}

	public void ExitPlacementMode()
	{
		state = BuildState.Idle;
		HideIndicators();
	}

	public void ExitDeconstructionMode()
	{
		state = BuildState.Disabled;
		HideIndicators();
		GameEvents.InvokeOnExitDeconstructionMode();
	}

	private (Tile tile, Entity tileInstance) GetTileUnderCursor()
	{
		Tile selectedTile = default;
		Entity inst = default;
		var col = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld;
		var ray = _cam.ScreenPointToRay(Input.mousePosition);
		if (col.CollisionWorld.CastRay(new Unity.Physics.RaycastInput
		{
			Start = ray.origin,
			End = ray.GetPoint(_cam.transform.position.y * 2),
			Filter = new Unity.Physics.CollisionFilter
			{
				GroupIndex = 0,
				BelongsTo = (uint)CollisionLayer.Tile,
				CollidesWith = (uint)CollisionLayer.Tile
			}
		}, out var hit))
		{
			if (hit.RigidBodyIndex != -1)
			{
				var e = col.Bodies[hit.RigidBodyIndex].Entity;
				if (GameRegistry.EntityManager.HasComponent<HexPosition>(e))
				{
					selectedTile = GameRegistry.GameMap[GameRegistry.EntityManager.GetComponentData<HexPosition>(e).Value];
					inst = e;
				}
			}
		}
		if (selectedTile is MetaTile m)
			selectedTile = m.ParentTile;
		return (selectedTile, inst);
	}

	private void ValidatePlacement()
	{
		if (_selectedBuilding == null)
		{
			state = BuildState.Idle;
			return;
		}
		indicatorManager.UnSetAllIndicators();
		indicatorManager.HideAllIndicators();
		var (selectedTile, _) = GetTileUnderCursor();
		if (selectedTile == null)
			return;
		if (Input.GetKeyUp(KeyCode.Q))
		{
			_rotation--;
		}
		else if (Input.GetKeyUp(KeyCode.E))
		{
			_rotation++;
		}
		_rotation = _rotation.Mod(6);

		Profiler.BeginSample("Validator");
		bool isValid = _selectedBuilding.validator.ValidatePlacement(GameRegistry.GameMap, selectedTile.Coords, _selectedBuilding, indicatorManager, _rotation);
		Profiler.EndSample();
		//var effects = new float2();

		var footprint = _selectedBuilding.footprint.GetOccupiedTiles(selectedTile.Coords, _rotation);

		for (int i = 0; i < footprint.Length; i++)
		{
			var t = footprint[i];
			var neighbors = GameRegistry.GameMap.GetNeighbors(t);
			//for (int j = 0; j < _selectedBuilding.adjacencyEffects.Length; j++)
			//	_selectedBuilding.adjacencyEffects[j].GetAdjacencyEffectsString(GameRegistry.GameMap[t], neighbors, ref effects);
		}

		ShowPoweredTiles(selectedTile);
		if (Input.GetKeyUp(KeyCode.Mouse0))
		{
			if (isValid)
				PlaceBuilding(selectedTile);
			else
				ShowErrors();
		}
	}

	//TODO: Optimize this
	private void ShowPoweredTiles(Tile selectedTile)
	{
		if (state == BuildState.HQPlacement)
			return;
		var poweredTiles = new List<Tile>(250);
		var unPoweredTiles = new List<Tile>(250);
		var conduitsInRange = GameRegistry.GameMap.conduitGraph.GetNodesInRange(selectedTile.Coords, _poweredTileRangeSq, false);
		for (int i = 0; i < conduitsInRange.Count; i++)
		{
			var conduit = conduitsInRange[i];
			if (conduit.IsConnected)
				poweredTiles.AddRange(GameRegistry.GameMap.HexSelect(conduit.conduitPos, conduit.poweredRange));
			else
				unPoweredTiles.AddRange(GameRegistry.GameMap.HexSelect(conduit.conduitPos, conduit.poweredRange));
		}
		if (poweredTiles.Count > 0)
			indicatorManager.ShowIndicators(poweredTileIndicatorPrefab, poweredTiles.Distinct().ToList());
		else
			indicatorManager.HideIndicator(poweredTileIndicatorPrefab);
		if (unPoweredTiles.Count > 0)
			indicatorManager.ShowIndicators(unpoweredTileIndicatorPrefab, unPoweredTiles.Distinct().ToList());
		else
			indicatorManager.HideIndicator(unpoweredTileIndicatorPrefab);
	}

	private void PlaceBuilding(Tile selectedTile)
	{
		if (state == BuildState.Placement)
			ResourceSystem.ConsumeResourses(_selectedBuilding.cost);
		BuildQueueSystem.QueueBuilding(_selectedBuilding, selectedTile, _rotation);
		if (state == BuildState.HQPlacement)
		{
			_selectedBuilding = null;
			floatingText.gameObject.SetActive(false);
			state = BuildState.Disabled;
			indicatorManager.HideAllIndicators();
			banner.SetActive(false);
		}
		if (_selectedBuilding is TechBuildingTileEntity)
		{
			_selectedBuilding = null;
			floatingText.gameObject.SetActive(false);
			state = BuildState.Idle;
			indicatorManager.HideAllIndicators();
		}
		indicatorManager.UnSetAllIndicators();
	}

	private void HideIndicators()
	{
		if (indicatorManager == null)
			return;
		indicatorManager.HideAllIndicators();
		indicatorManager.UnSetAllIndicators();
	}

	private void ShowErrors()
	{
		/*
			for (int i = 0; i < _errors.Count; i++)
			NotificationsUI.Notify(NotifType.Error, _errors[i]);
		_errors.Clear();
		*/
		indicatorManager.PublishAndClearErrors();
	}
}