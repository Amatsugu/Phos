using DataStore.ConduitGraph;

using Effects.Lines;

using System.Collections.Generic;
using System.Linq;

using TMPro;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildUI : MonoBehaviour
{
	[Header("Buildings")]
	public BuildingDatabase buildings;

	public HQTileInfo HQTile;
	public MeshEntityRotatable landingMesh;

	/*	UI	*/

	[Header("UI")]
	public UIInfoBanner infoBanner;

	public RectTransform unitUIPrefab;
	public GameObject buildWindow;
	public RectTransform scrollContent;
	public GraphicRaycaster raycaster;
	public EventSystem eventSystem;

	//Indicators
	[Header("Indicators")]
	public MeshEntity selectIndicatorEntity;

	public MeshEntity placementPathIndicatorEntity;
	public MeshEntity poweredTileIndicatorEntity;
	public MeshEntity unpoweredTileIndicatorEntity;
	public MeshEntity errorIndicatorEntity;

	//Tooltip
	[Header("Tooltip")]
	public UIBuildingTooltip toolTip;

	public TMP_Text floatingText;

	[Header("Config")]
	public int poweredTileDisplayRange;

	public float inidcatorOffset = .5f;

	//State
	public BuildState State { get; private set; }

	public event System.Action OnBuildWindowOpen;

	public event System.Action OnBuildWindowClose;

	private List<UIUnitIcon> _activeUnits;
	private BuildingTileEntity _selectedBuilding;
	private Camera _cam;
	

	private List<Tile> _buildPath;
	private bool _validPlacement;
	private BuildingCategory? _lastBuildingCategory;

	private float _poweredTileRangeSq;
	private List<string> _errors;
	private BuildState _prevState;
	private IndicatorManager _indicatorManager;
	private BuildPhysicsWorld _physWorld;

	public enum BuildState
	{
		Disabled = 0,
		Idle,
		HQPlacement,
		Placement
	}

	private void Awake()
	{
		GameRegistry.INST.buildUI = this;
		GameRegistry.SetBuildingDatabase(buildings);
		GameEvents.OnGameReady += Init;
		enabled = false;
		GameEvents.OnMapRegen += OnRegen;
		_physWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
	}

	private void Init()
	{
		enabled = true;
		UnityEngine.Debug.Log($"BuildUI Init");
		GameEvents.OnGameReady -= Init;
		_indicatorManager = _indicatorManager ?? new IndicatorManager(Map.EM, inidcatorOffset, floatingText);
		_activeUnits = _activeUnits ?? new List<UIUnitIcon>();
		HideBuildWindow();
		infoBanner.SetText("Place HQ Building");
		_prevState = State = BuildState.HQPlacement;
		_selectedBuilding = HQTile;
	}

	private void Start()
	{
		UnityEngine.Debug.Log("Build UI Start");
		_errors = new List<string>();
		_cam = GameRegistry.Camera;
		/*_pendingBuildOrders.Values.Any(o => o.dstTile == t) ||*/

		if (selectIndicatorEntity.mesh == null || selectIndicatorEntity.material == null)
			UnityEngine.Debug.LogError("Null");

		// TODO: Improve this
		EventManager.AddEventListener(GameEvent.OnBuildingUnlocked.ToString(), () =>
		{
			if (_lastBuildingCategory != null)
				ShowBuildWindow(buildings[(BuildingCategory)_lastBuildingCategory]);
		});
		_poweredTileRangeSq = HexCoords.TileToWorldDist(poweredTileDisplayRange, Map.ActiveMap.innerRadius);
		_poweredTileRangeSq *= _poweredTileRangeSq;
	}

	private void OnRegen()
	{
		GameEvents.OnGameReady += Init;
	}

	private bool InvalidPlacementSelector(Tile t) => (_selectedBuilding.offshoreOnly ? !t.IsUnderwater : (t.IsUnderwater && !_selectedBuilding.isOffshore)) || t is BuildingTile || (t is ResourceTile && !t.IsUnderwater);

	// Update is called once per frame
	private void Update()
	{
#if DEBUG
		if (_cam == null)
			_cam = GameRegistry.Camera;
#endif

		var mPos = Input.mousePosition;
		var pointerData = new PointerEventData(eventSystem)
		{
			position = mPos
		};
		var results = new List<RaycastResult>();
		raycaster.Raycast(pointerData, results);
		if (results.Count > 0)
		{
			if (State > BuildState.Idle)
			{
				_prevState = State;
				State = BuildState.Idle;
				_indicatorManager.HideAllIndicators();
				floatingText.gameObject.SetActive(false);
			}
		}
		else
		{
			if (State == BuildState.Idle)
			{
				State = _prevState;
				_prevState = BuildState.Idle;
			}
		}
		switch (State)
		{
			case BuildState.Disabled:
				break;

			case BuildState.Idle:
				ReadCloseInput();
				if (Input.GetKeyDown(KeyCode.Mouse0) && results.Count == 0)
					HideBuildWindow();
				break;

			case BuildState.HQPlacement:
				UpdatePlacementUI(mPos);
				break;

			case BuildState.Placement:
				UpdatePlacementUI(mPos);
				ReadDeselectBuildingInput();
				break;
		}
	}

	private void UpdatePlacementUI(Vector2 mousePos)
	{
		if (_selectedBuilding == null)
		{
			State = _prevState = BuildState.Idle;
			_indicatorManager.HideAllIndicators();
			floatingText.gameObject.SetActive(false);
		}
		Tile selectedTile = null;
		var col = _physWorld.PhysicsWorld;
		var ray = _cam.ScreenPointToRay(mousePos);
		var hasTile = _physWorld.GetTileFromRay(ray, _cam.transform.position.y * 2, out var pos);
		if (hasTile)
			selectedTile = Map.ActiveMap[pos];
#if DEBUG
		Debug.DrawLine(ray.origin, ray.GetPoint(_cam.transform.position.y * 2));
#endif
		if (selectedTile == null)
		{
			_indicatorManager.HideAllIndicators();
			floatingText.gameObject.SetActive(false);
			return;
		}
		_errors.Clear();
		_validPlacement = true;
		var tilesToOccupy = Map.ActiveMap.HexSelect(selectedTile.Coords, _selectedBuilding.size);
		_indicatorManager.UnSetAllIndicators();
		_indicatorManager.floatingText.gameObject.SetActive(false);
		_validPlacement = _selectedBuilding.validator.ValidatePlacement(Map.ActiveMap, selectedTile.Coords, _selectedBuilding, _indicatorManager);
		ValidatePlacement(tilesToOccupy);
		ShowPoweredTiles(selectedTile);
		if (Input.GetKeyDown(KeyCode.Mouse0))
			PlaceBuilding(selectedTile);
	}

	private void ValidatePlacement(List<Tile> tilesToOccupy)
	{
		if (!GameRegistry.ResourceSystem.HasAllResources(_selectedBuilding.cost) && State != BuildState.HQPlacement) //Has Resources
		{
			_indicatorManager.HideIndicator(selectIndicatorEntity);
			_indicatorManager.ShowIndicators(errorIndicatorEntity, tilesToOccupy);
			_validPlacement = false;
			_errors.Add("Insuffient Resources");
		}
		else if (tilesToOccupy.Any(InvalidPlacementSelector)) //Valid Placement
		{
			var invalidTiles = tilesToOccupy.Where(InvalidPlacementSelector);
			_indicatorManager.ShowIndicators(errorIndicatorEntity, invalidTiles.ToList());
			_indicatorManager.ShowIndicators(selectIndicatorEntity, tilesToOccupy.Except(invalidTiles).ToList());
			_validPlacement = false;
			_errors.Add("Cannot place on these tiles.");
		}
		else
		{
			_indicatorManager.HideIndicator(errorIndicatorEntity);
			_indicatorManager.ShowIndicators(selectIndicatorEntity, tilesToOccupy);
		}
	}

	private void PlaceBuilding(Tile selectedTile)
	{
		if (_validPlacement)
		{
			if (State == BuildState.Placement)
				ResourceSystem.ConsumeResourses(_selectedBuilding.cost);
			if (_buildPath == null)
			{
				BuildQueueSystem.QueueBuilding(_selectedBuilding, selectedTile, landingMesh);
			}
			else
			{
				for (int i = 0; i < _buildPath.Count; i++)
				{
					if (InvalidPlacementSelector(_buildPath[i]))
						continue;
					BuildQueueSystem.QueueBuilding(_selectedBuilding, _buildPath[i], landingMesh);
				}
				_buildPath = null;
			}
			if (State == BuildState.HQPlacement)
			{
				_selectedBuilding = null;
				_indicatorManager.HideAllIndicators();
				floatingText.gameObject.SetActive(false);
				State = BuildState.Disabled;
				infoBanner.SetActive(false);
				void onHide()
				{
					GameRegistry.BaseNameUI.panel.OnHide -= onHide;
				}
				GameRegistry.BaseNameUI.panel.OnHide += onHide;
			}
		}
		else
		{
			//TODO: Add more detailed error
			for (int i = 0; i < _errors.Count; i++)
				NotificationsUI.Notify(NotifType.Error, _errors[i]);
			_errors.Clear();
			_indicatorManager.PublishAndClearErrors();
		}
		_indicatorManager.UnSetAllIndicators();
	}

	private void ReadCloseInput()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			HideBuildWindow();
		}
	}

	private void ReadDeselectBuildingInput()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			State = BuildState.Idle;
			_prevState = BuildState.Idle;
			_indicatorManager.HideAllIndicators();
			floatingText.gameObject.SetActive(false);
		}
	}

	private void ShowPoweredTiles(Tile selectedTile)
	{
		if (State == BuildState.HQPlacement)
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

	public void ShowBuildWindow(BuildingDatabase.BuildingDefination[] buildings)
	{
		OnBuildWindowOpen?.Invoke();
		if (State == BuildState.HQPlacement)
			return;
		GameRegistry.InteractionUI.interactionPanel.HidePanel();
		buildWindow.SetActive(true);
		State = BuildState.Idle;
		_selectedBuilding = null;
		if (_activeUnits.Count < buildings.Length)
		{
			GrowUnitsUI(buildings.Length);
		}
		var activeCount = 0;
		for (int i = 0; i < buildings.Length; i++)
		{
			if (!GameRegistry.IsBuildingUnlocked(buildings[i].Id))
			{
				_activeUnits[i].gameObject.SetActive(false);
				continue;
			}
			var building = buildings[i].info;
#if DEBUG
			if (building == null)
			{
				UnityEngine.Debug.LogWarning("Null building in list, aborting");
				break;
			}
#endif
			_activeUnits[i].gameObject.SetActive(true);
			activeCount++;
			_activeUnits[i].costText.SetText(building.name);
			_activeUnits[i].ClearAllEvents();
			_activeUnits[i].OnClick += () =>
			{
				if (GameRegistry.ResourceSystem.HasAllResources(building.cost))
				{
					_selectedBuilding = building;
					State = BuildState.Placement;
				}
			};
			_activeUnits[i].OnHover += () =>
				toolTip.ShowToolTip(building.icon,
									building.name,
									building.description,
									building.GetCostString(),
									building.GetProductionString());
			_activeUnits[i].OnBlur += () => toolTip.Hide();
			_activeUnits[i].icon.sprite = building.icon;
		}
		for (int i = buildings.Length; i < _activeUnits.Count; i++)
		{
			_activeUnits[i].gameObject.SetActive(false);
		}
		scrollContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 5 + (activeCount * (_activeUnits[0].rTransform.rect.width + 10)));
	}

	public void GrowUnitsUI(int count)
	{
		for (int i = _activeUnits.Count; i < count; i++)
		{
			var unit = Instantiate(unitUIPrefab, scrollContent).GetComponent<UIUnitIcon>();
			unit.anchoredPosition = new Vector2(10 + (i * 170), 5);
			_activeUnits.Add(unit);
		}
	}

	public void ShowTechWindow() => ShowBuildWindow(buildings[(BuildingCategory)(_lastBuildingCategory = BuildingCategory.Tech)]);

	public void ShowResourcesWindow() => ShowBuildWindow(buildings[(BuildingCategory)(_lastBuildingCategory = BuildingCategory.Gathering)]);

	public void ShowProdcutionWindow() => ShowBuildWindow(buildings[(BuildingCategory)(_lastBuildingCategory = BuildingCategory.Production)]);

	public void ShowStructureWindow() => ShowBuildWindow(buildings[(BuildingCategory)(_lastBuildingCategory = BuildingCategory.Structure)]);

	public void ShowMilitaryWindow() => ShowBuildWindow(buildings[(BuildingCategory)(_lastBuildingCategory = BuildingCategory.Military)]);

	public void ShowDefenseWindow() => ShowBuildWindow(buildings[(BuildingCategory)(_lastBuildingCategory = BuildingCategory.Defense)]);

	public void HideBuildWindow()
	{
		OnBuildWindowClose?.Invoke();
		_indicatorManager.HideAllIndicators();
		_indicatorManager.UnSetAllIndicators();
		State = BuildState.Disabled;
		_lastBuildingCategory = null;
		_selectedBuilding = null;
		buildWindow.SetActive(false);
		floatingText.gameObject.SetActive(false);
		for (int i = 0; i < _activeUnits.Count; i++)
		{
			_activeUnits[i]?.gameObject.SetActive(false);
		}
	}
}