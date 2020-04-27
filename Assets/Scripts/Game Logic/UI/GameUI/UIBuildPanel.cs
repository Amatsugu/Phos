using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Entities;
using Unity.Physics.Systems;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildPanel : UITabPanel
{
	public UIUnitIcon iconPrefab;
	public BuildingDatabase buildingDatabase;
	[HideInInspector]
	public UIInfoPanel infoPanel;
	public TMP_Text floatingText;
	public UIInfoBanner banner;
	public float inidcatorOffset = .5f;
	public HQTileInfo hQTile;
	public MeshEntityRotatable dropPod;
	public RectTransform contentArea;
	public float showPowerRange = 20;

	[Header("Indicators")]
	public MeshEntity poweredTileIndicatorEntity;
	public MeshEntity unpoweredTileIndicatorEntity;

	[HideInInspector]
	public BuildState state;



	private UIUnitIcon[] _icons;
	private BuildingTileEntity _selectedBuilding;
	private int _tier = 1;
	private BuildingCategory _lastCategory;
	private IndicatorManager _indicatorManager;
	private float _poweredTileRangeSq;
	private Camera _cam;

	public enum BuildState
	{
		Disabled = 0,
		Idle,
		HQPlacement,
		Placement
	}

	protected override void Awake()
	{
		GameRegistry.SetBuildingDatabase(buildingDatabase);
		OnHide += () =>
		{
			if (state == BuildState.HQPlacement)
				return;
			HideIndicators();
			state = BuildState.Disabled;
		};
		_icons = new UIUnitIcon[8];
		GameEvents.OnGameReady += OnGameReady;
		_poweredTileRangeSq = showPowerRange * showPowerRange;
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		_cam = GameRegistry.Camera;
		_indicatorManager = new IndicatorManager(Map.EM, inidcatorOffset, floatingText);
	}

	void OnGameReady()
	{
		state = BuildState.HQPlacement;
		_selectedBuilding = hQTile;
		GameEvents.OnGameTick += OnTick;
	}

	protected override void OnTabSelected(int tab)
	{
		base.OnTabSelected(tab);
		_tier = tab + 1;
		Show(_lastCategory);
	}

	private void OnTick()
	{
		if (state == BuildState.Disabled || state == BuildState.HQPlacement)
			return;
		var buildings = buildingDatabase[_lastCategory];
		for (int i = 0, j = 0; i < _icons.Length; i++)
		{
			if (_icons[i] == null)
			{
				_icons[i] = Instantiate(iconPrefab, contentArea, false);
				_icons[i].OnBlur += infoPanel.Hide;
			}
			if (j < buildings.Length)
			{
				if (buildings[j].info.tier == _tier)
				{
					_icons[i].SetActive(false);
					_icons[i].ClearHoverEvents();
					_icons[i].ClearClickEvents();
					_icons[i].titleText.text = buildings[j].info.name;
					_icons[i].costText.text = buildings[j].info.GetCostString();
					_icons[i].icon.sprite = buildings[j].info.icon;
					var b = buildings[j];
					_icons[i].OnHover += () => infoPanel.ShowInfo(b);
					_icons[i].OnClick += () =>
					{
						state = BuildState.Placement;
						_selectedBuilding = b.info;
					};
					_icons[i].SetActive(true);
				}
				else
					i--;
				j++;
			}
			else
				_icons[i].SetActive(false);
		}
	}

	public void Show(BuildingCategory category)
	{
		if (state == BuildState.HQPlacement)
			return;
		state = BuildState.Idle;
		_lastCategory = category;
		var buildings = buildingDatabase[category];
		for (int i = 0, j = 0; i < _icons.Length; i++)
		{
			if (_icons[i] == null)
			{
				_icons[i] = Instantiate(iconPrefab, contentArea, false);
				_icons[i].OnBlur += infoPanel.Hide;
			}
			if (j < buildings.Length)
			{
				if (buildings[j].info.tier == _tier)
				{
					_icons[i].SetActive(false);
					_icons[i].ClearHoverEvents();
					_icons[i].ClearClickEvents();
					_icons[i].titleText.text = buildings[j].info.name;
					_icons[i].costText.text = buildings[j].info.GetCostString();
					_icons[i].icon.sprite = buildings[j].info.icon;
					var b = buildings[j];
					_icons[i].OnHover += () => infoPanel.ShowInfo(b);
					_icons[i].OnClick += () =>
					{
						state = BuildState.Placement;
						_selectedBuilding = b.info;
					};
					_icons[i].SetActive(true);
				}
				else
					i--;
				j++;
			}else
				_icons[i].SetActive(false);
		}
		Show();
	}

	public void UpdateState()
	{
		switch(state)
		{
			case BuildState.Disabled:

				break;
			case BuildState.Idle:
				ReadBackInput();
				break;
			case BuildState.HQPlacement:
				ValidatePlacement();
				break;
			case BuildState.Placement:
				ValidatePlacement();
				ReadBackInput();
				break;
		}
	}

	void ReadBackInput()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if(state == BuildState.Placement)
			{
				state = BuildState.Idle;
				HideIndicators();	
			}
			else
			{
				Hide();
			}
		}
	}

	Tile GetTileUnderCursor()
	{
		Tile selectedTile = default;
		var col = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;
		var ray = _cam.ScreenPointToRay(Input.mousePosition);
		if (col.CollisionWorld.CastRay(new Unity.Physics.RaycastInput
		{
			Start = ray.origin,
			End = ray.GetPoint(_cam.transform.position.y * 2),
			Filter = new Unity.Physics.CollisionFilter
			{
				GroupIndex = 0,
				BelongsTo = (1u << (int)Faction.Tile),
				CollidesWith = (1u << (int)Faction.Tile)
			}
		}, out var hit))
		{
			if (hit.RigidBodyIndex != -1)
			{
				var e = col.Bodies[hit.RigidBodyIndex].Entity;
				if (Map.EM.HasComponent<HexPosition>(e))
					selectedTile = Map.ActiveMap[Map.EM.GetComponentData<HexPosition>(e).Value];
			}
		}
		return selectedTile;
	}

	void ValidatePlacement()
	{
		if(_selectedBuilding == null)
		{
			state = BuildState.Idle;
			return;
		}
		_indicatorManager.UnSetAllIndicators();
		_indicatorManager.HideAllIndicators();
		var selectedTile = GetTileUnderCursor();
		if (selectedTile == null)
			return;
		bool isValid = _selectedBuilding.validator.ValidatePlacement(Map.ActiveMap, selectedTile.Coords, _selectedBuilding, _indicatorManager);
		var neighbors = Map.ActiveMap.GetNeighbors(selectedTile.Coords);
		var effects = new List<string>();
		for (int i = 0; i < _selectedBuilding.adjacencyEffects.Length; i++)
			_selectedBuilding.adjacencyEffects[i].GetAdjacencyEffectsString(_selectedBuilding, neighbors, ref effects);

		Debug.Log("--Effects--");
		for (int i = 0; i < effects.Count; i++)
			Debug.Log(effects[i]);
		Debug.Log("-----------");

		ShowPoweredTiles(selectedTile);
		if(Input.GetKeyUp(KeyCode.Mouse0))
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
		var conduitsInRange = Map.ActiveMap.conduitGraph.GetNodesInRange(selectedTile.Coords, _poweredTileRangeSq, false);
		for (int i = 0; i < conduitsInRange.Count; i++)
		{
			var conduit = (Map.ActiveMap[conduitsInRange[i].conduitPos] as ResourceConduitTile);
			if (conduit == null)
				continue;
			if (conduit.HasHQConnection)
				poweredTiles.AddRange(Map.ActiveMap.HexSelect(conduit.Coords, conduit.conduitInfo.poweredRange));
			else
				unPoweredTiles.AddRange(Map.ActiveMap.HexSelect(conduit.Coords, conduit.conduitInfo.poweredRange));
		}
		if (poweredTiles.Count > 0)
			_indicatorManager.ShowIndicators(poweredTileIndicatorEntity, poweredTiles.Distinct().ToList());
		else
			_indicatorManager.HideIndicator(poweredTileIndicatorEntity);
		if (unPoweredTiles.Count > 0)
			_indicatorManager.ShowIndicators(unpoweredTileIndicatorEntity, unPoweredTiles.Distinct().ToList());
		else
			_indicatorManager.HideIndicator(unpoweredTileIndicatorEntity);
	}

	private void PlaceBuilding(Tile selectedTile)
	{
		if (state == BuildState.Placement)
			ResourceSystem.ConsumeResourses(_selectedBuilding.cost);
		BuildQueueSystem.QueueBuilding(_selectedBuilding, selectedTile, dropPod);
		if (state == BuildState.HQPlacement)
		{
			_selectedBuilding = null;
			floatingText.gameObject.SetActive(false);
			state = BuildState.Disabled;
			_indicatorManager.HideAllIndicators();
			banner.SetActive(false);
		}
		_indicatorManager.UnSetAllIndicators();
	}

	void HideIndicators()
	{
		_indicatorManager.HideAllIndicators();
		_indicatorManager.UnSetAllIndicators();
	}

	private void ShowErrors()
	{
		/*
			for (int i = 0; i < _errors.Count; i++)
			NotificationsUI.Notify(NotifType.Error, _errors[i]);
		_errors.Clear();
		*/
		_indicatorManager.PublishAndClearErrors();
	}

}