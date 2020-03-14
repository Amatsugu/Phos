using Amatsugu.Phos.ECS.Jobs.Pathfinder;

using DataStore.ConduitGraph;

using Effects.Lines;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using TMPro;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

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
	public MeshEntity gatheringIndicatorEntity;
	public MeshEntity cannotGatheringIndicatorEntity;
	public MeshEntity powerIndicatorEntity;
	public MeshEntity poweredTileIndicatorEntity;
	public MeshEntity unpoweredTileIndicatorEntity;
	public MeshEntity errorIndicatorEntity;
	public MeshEntityRotatable resourceConduitPreviewLine;

	//Tooltip
	[Header("Tooltip")]
	public UIBuildingTooltip toolTip;

	public TMP_Text floatingText;

	[Header("Config")]
	public int poweredTileDisplayRange;
	public float inidcatorOffset = .5f;

	public BuildState State { get => _state; }


	private List<UIUnitIcon> _activeUnits;
	private BuildingTileInfo _selectedBuilding;
	private Camera _cam;
	private Dictionary<MeshEntity, List<Entity>> _indicatorEntities;
	private Dictionary<MeshEntity, int> _renderedEntities;
	private EntityManager _EM;

	private Tile _startPoint;
	private List<Tile> _buildPath;
	private bool _suffientFunds;
	private bool _validPlacement;
	private BuildingCategory? _lastBuildingCategory;

	private float _poweredTileRangeSq;
	private List<string> _errors;
	private BuildState _state;
	private BuildState _prevState;

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
	}

	private void Start()
	{
		_errors = new List<string>();
		_indicatorEntities = new Dictionary<MeshEntity, List<Entity>>();
		_renderedEntities = new Dictionary<MeshEntity, int>();
		_activeUnits = new List<UIUnitIcon>();
		_cam = GameRegistry.Camera;
		_EM = World.DefaultGameObjectInjectionWorld.EntityManager;
		HideBuildWindow();
		_state = BuildState.HQPlacement;
		_selectedBuilding = HQTile;
		infoBanner.SetText("Place HQ Building");
		/*_pendingBuildOrders.Values.Any(o => o.dstTile == t) ||*/

		if (selectIndicatorEntity.mesh == null || selectIndicatorEntity.material == null)
			Debug.LogError("Null");


		EventManager.AddEventListener(GameEvent.OnBuildingUnlocked, () =>
		{
			if (_lastBuildingCategory != null)
				ShowBuildWindow(buildings[(BuildingCategory)_lastBuildingCategory]);
		});
		_poweredTileRangeSq = HexCoords.TileToWorldDist(poweredTileDisplayRange, Map.ActiveMap.innerRadius);
		_poweredTileRangeSq *= _poweredTileRangeSq;

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
		if(results.Count > 0)
		{
			if (_state > BuildState.Idle)
			{
				_prevState = _state;
				_state = BuildState.Idle;
			}
		}else
		{
			if (_state == BuildState.Idle)
			{
				_state = _prevState;
				_prevState = BuildState.Idle;
			}
		}
		switch (_state)
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
		Tile selectedTile = null;//Map.ActiveMap.GetTileFromRay(_cam.ScreenPointToRay(mousePos), _cam.transform.position.y * 2);
		var col = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;
		var ray = _cam.ScreenPointToRay(mousePos);
		if (col.CollisionWorld.CastRay(new Unity.Physics.RaycastInput
		{
			Start = ray.origin,
			End = ray.GetPoint(_cam.transform.position.y * 2),
			Filter = new Unity.Physics.CollisionFilter
			{
				BelongsTo = ~0u,
				CollidesWith = ~0u,
				GroupIndex = 0
			}
		}, out var hit))
		{
			if (hit.RigidBodyIndex != -1)
			{
				var e = col.Bodies[hit.RigidBodyIndex].Entity;
				if (Map.EM.HasComponent<HexPosition>(e))
					selectedTile = Map.ActiveMap[Map.EM.GetComponentData<HexPosition>(e).coords];
			}
		}
		Debug.DrawRay(hit.Position, hit.SurfaceNormal * 5, Color.cyan);
		Debug.DrawRay(hit.Position, Vector3.right, Color.red);
		Debug.DrawRay(hit.Position, Vector3.forward, Color.blue);
		Debug.DrawRay(hit.Position, Vector3.up, Color.green);
		Debug.DrawLine(ray.origin, ray.GetPoint(_cam.transform.position.y * 2));

		if (selectedTile == null)
		{
			HideAllIndicators();
			return;
		}
		_errors.Clear();
		_validPlacement = true;
		_suffientFunds = true;
		var tilesToOccupy = Map.ActiveMap.HexSelect(selectedTile.Coords, _selectedBuilding.size);
		ValidatePathPlacement(selectedTile);
		ValidatePlacement(tilesToOccupy);
		ShowPoweredTiles(selectedTile);
		switch (_selectedBuilding)
		{
			case ResourceConduitTileInfo conduit:
				ValidateResourceConduit(selectedTile, conduit);
				break;

			case ResourceGatheringBuildingInfo building:
				ValidateResourceGatheringBuilding(selectedTile, tilesToOccupy, building);
				break;
		}
		if (Input.GetKeyDown(KeyCode.Mouse0))
			PlaceBuilding(selectedTile);
	}

	private void ValidatePathPlacement(Tile selectedTile)
	{
		if (_selectedBuilding.placementMode != PlacementMode.Path)
			return;
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			_startPoint = selectedTile;
		}
		if (Input.GetKey(KeyCode.Mouse0) && _startPoint != null)
		{
			//Target tile is valid?
			if (InvalidPlacementSelector(selectedTile))
			{
				HideIndicator(placementPathIndicatorEntity);
				_buildPath = null;
			}
			if (!(selectedTile is ResourceTile))
			{
				_buildPath = Map.ActiveMap.GetPath(_startPoint, selectedTile, filter: t => !(t is ResourceTile));
			}
			else
				_buildPath = null;
			if (_buildPath != null)
			{
				if (_buildPath.Any(t => t.IsUnderwater))
				{
					var invalidTiles = _buildPath.Where(t => t.IsUnderwater);
					ShowIndicators(errorIndicatorEntity, invalidTiles.ToList());
					ShowIndicators(selectIndicatorEntity, _buildPath.Except(invalidTiles).ToList());
					_validPlacement = false;
				}
				else
				{
					HideIndicator(errorIndicatorEntity);
					ShowIndicators(placementPathIndicatorEntity, _buildPath);
					_validPlacement = false;
				}
			}
		}
	}

	private void ValidatePlacement(List<Tile> tilesToOccupy)
	{
		if (!GameRegistry.ResourceSystem.HasAllResources(_selectedBuilding.cost)) //Has Resources
		{
			HideIndicator(selectIndicatorEntity);
			ShowIndicators(errorIndicatorEntity, tilesToOccupy);
			_validPlacement = false;
			_suffientFunds = false;
			_errors.Add("Insuffient Resources");
		}
		else if (tilesToOccupy.Any(InvalidPlacementSelector)) //Valid Placement
		{
			var invalidTiles = tilesToOccupy.Where(InvalidPlacementSelector);
			ShowIndicators(errorIndicatorEntity, invalidTiles.ToList());
			ShowIndicators(selectIndicatorEntity, tilesToOccupy.Except(invalidTiles).ToList());
			_validPlacement = false;
			_errors.Add("Cannot place on these tiles.");
		}
		else
		{
			HideIndicator(errorIndicatorEntity);
			ShowIndicators(selectIndicatorEntity, tilesToOccupy);
		}
	}

	private void PlaceBuilding(Tile selectedTile)
	{
		_startPoint = null;
		if (_validPlacement || _selectedBuilding.placementMode == PlacementMode.Path)
		{
			if (_state == BuildState.Placement)
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
			if (_state == BuildState.HQPlacement)
			{
				_selectedBuilding = null;
				HideAllIndicators();
				_state = BuildState.Disabled;
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
		}
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
			_state = BuildState.Idle;
			_prevState = BuildState.Idle;
			HideAllIndicators();
		}
	}

	private void ShowPoweredTiles(Tile selectedTile)
	{
		if (_state == BuildState.HQPlacement)
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
			ShowIndicators(poweredTileIndicatorEntity, poweredTiles.Distinct().ToList());
		else
			HideIndicator(poweredTileIndicatorEntity);
		if (unPoweredTiles.Count > 0)
			ShowIndicators(unpoweredTileIndicatorEntity, unPoweredTiles.Distinct().ToList());
		else
			HideIndicator(unpoweredTileIndicatorEntity);
	}

	private void ValidateResourceConduit(Tile selectedTile, ResourceConduitTileInfo conduitInfo)
	{
		var rangeSqr = HexCoords.TileToWorldDist(conduitInfo.connectionRange, Map.ActiveMap.innerRadius);
		rangeSqr *= rangeSqr;
		var nodes = Map.ActiveMap.conduitGraph.GetNodesInRange(selectedTile.Coords, rangeSqr);
		while (nodes.Count > Map.ActiveMap.conduitGraph.maxConnections)
			nodes.RemoveAt(nodes.Count - 1);
		nodes.RemoveAll(n => n.conduitPos == selectedTile.Coords);
		ShowLines(resourceConduitPreviewLine, selectedTile.SurfacePoint + new float3(0, conduitInfo.powerLineOffset, 0), nodes);
#if DEBUG
		for (int i = 0; i < nodes.Count; i++)
		{
			Debug.DrawLine(selectedTile.SurfacePoint, Map.ActiveMap[nodes[i].conduitPos].SurfacePoint, Color.cyan);
		}
#endif
		//else
		//	HideIndicator(resourceConduitPreviewLine);
		ShowIndicators(powerIndicatorEntity, Map.ActiveMap.HexSelect(selectedTile.Coords, conduitInfo.poweredRange, true));
	}

	private void ShowLines(MeshEntityRotatable line, Vector3 src, List<ConduitNode> nodes, float thiccness = 0.1f)
	{
		GrowIndicators(line, nodes.Count);
		int c = 0;
		for (int i = 0, j = 0; i < _indicatorEntities[line].Count; i++, j++)
		{
			if (j < nodes.Count)
			{
				if (i >= _renderedEntities[line])
					_EM.RemoveComponent<FrozenRenderSceneTag>(_indicatorEntities[line][i]);

				var pos = nodes[j].conduitPos.worldXZ + new Vector3(0, nodes[j].height, 0);
				LineFactory.UpdateStaticLine(_indicatorEntities[line][i], src, pos, thiccness);
				c++;
			}
			else
			{
				if (i >= _renderedEntities[line])
					break;
				if (i < _renderedEntities[line])
					_EM.AddComponent(_indicatorEntities[line][i], typeof(FrozenRenderSceneTag));
			}
		}
		_renderedEntities[line] = c;
	}

	private void ValidateResourceGatheringBuilding(Tile selectedTile, List<Tile> tilesToOccupy, ResourceGatheringBuildingInfo buildingInfo)
	{
		var resInRange = new Dictionary<int, int>();
		var resTiles = new Dictionary<int, List<Tile>>();
		Map.ActiveMap.HexSelectForEach(selectedTile.Coords, buildingInfo.size + buildingInfo.gatherRange, t =>
		{
			if (t is ResourceTile rt && !rt.gatherer.isCreated)
			{
				var yeild = rt.resInfo.resourceYields;
				for (int i = 0; i < yeild.Length; i++)
				{
					var yID = yeild[i].id;
					if (resInRange.ContainsKey(yID))
					{
						resInRange[yID]++;
						resTiles[yID].Add(t);
					}
					else
					{
						resInRange.Add(yID, 1);
						resTiles.Add(yID, new List<Tile> { t });
					}
				}
			}
		}, true);

		var cannotGatherTiles = new List<Tile>();
		var gatheredTiles = new List<Tile>();
		var gatherText = new System.Text.StringBuilder();

		for (int i = 0; i < buildingInfo.resourcesToGather.Length; i++)
		{
			var res = buildingInfo.resourcesToGather[i];
			if (!resInRange.ContainsKey(res.id))
				continue;
			if (resInRange.Count > 0)
			{
				var gatherAmmount = Mathf.FloorToInt(resInRange[res.id] * res.ammount);
				gatheredTiles.AddRange(resTiles[res.id]);
				gatherText.AppendLine($"+{gatherAmmount}{ResourceDatabase.GetResourceString(res.id)}");
			}
			resTiles.Remove(res.id);
		}
		foreach (var tiles in resTiles)
			cannotGatherTiles.AddRange(tiles.Value);
		if (gatheredTiles.Count > 0)
		{
			floatingText.SetText(gatherText);
			var pos = _cam.WorldToScreenPoint(selectedTile.SurfacePoint);
			pos.y += 20;
			floatingText.rectTransform.position = pos;
			floatingText.gameObject.SetActive(true);
			ShowIndicators(gatheringIndicatorEntity, gatheredTiles);
			ShowIndicators(cannotGatheringIndicatorEntity, cannotGatherTiles);
			if (!InvalidPlacementSelector(selectedTile) && _suffientFunds)
				HideIndicator(errorIndicatorEntity);
		}
		else
		{
			_validPlacement = false;
			if (cannotGatherTiles.Count > 0)
				_errors.Add("Building cannot gather these resources");
			else
				_errors.Add("No resources to gather");
			HideIndicator(gatheringIndicatorEntity);
			HideIndicator(selectIndicatorEntity);
			ShowIndicators(errorIndicatorEntity, tilesToOccupy);
			ShowIndicators(cannotGatheringIndicatorEntity, cannotGatherTiles);
			floatingText.gameObject.SetActive(false);
		}
	}

	private void HideIndicator(MeshEntity indicator)
	{
		if (!_indicatorEntities.ContainsKey(indicator))
			return;
		for (int i = 0; i < _renderedEntities[indicator]; i++)
		{
			_EM.AddComponent(_indicatorEntities[indicator][i], typeof(FrozenRenderSceneTag));
		}
		_renderedEntities[indicator] = 0;
	}

	private void HideAllIndicators()
	{
		foreach (var indicators in _indicatorEntities)
		{
			HideIndicator(indicators.Key);
		}
		floatingText.gameObject.SetActive(false);
	}

	private void OnDisable()
	{
	}

	private void GrowIndicators(MeshEntity indicatorMesh, int count)
	{
		List<Entity> entities;
		if (!_indicatorEntities.ContainsKey(indicatorMesh))
		{
			_indicatorEntities.Add(indicatorMesh, entities = new List<Entity>());
			_renderedEntities.Add(indicatorMesh, 0);
		}
		else
		{
			entities = _indicatorEntities[indicatorMesh];
			if (count <= entities.Count)
				return;
		}
		var curSize = entities.Count;
		for (int i = curSize; i < count; i++)
		{
			Entity curEntity;
			entities.Add(curEntity = indicatorMesh.Instantiate(Vector3.zero, Vector3.one * .9f));
			Map.EM.AddComponent(curEntity, typeof(FrozenRenderSceneTag));
		}
	}

	private void ShowIndicators(MeshEntity indicatorMesh, List<Tile> tiles)
	{
		GrowIndicators(indicatorMesh, tiles.Count);
		for (int i = 0; i < _indicatorEntities[indicatorMesh].Count; i++)
		{
			if (i < tiles.Count)
			{
				if (i >= _renderedEntities[indicatorMesh])
					_EM.RemoveComponent<FrozenRenderSceneTag>(_indicatorEntities[indicatorMesh][i]);

				_EM.SetComponentData(_indicatorEntities[indicatorMesh][i], new Translation { Value = tiles[i].SurfacePoint + new float3(0, inidcatorOffset, 0) });
			}
			else
			{
				if (i >= _renderedEntities[indicatorMesh])
					break;
				if (i < _renderedEntities[indicatorMesh])
					_EM.AddComponent(_indicatorEntities[indicatorMesh][i], typeof(FrozenRenderSceneTag));
			}
		}
		_renderedEntities[indicatorMesh] = tiles.Count;
	}

	public void ShowBuildWindow(BuildingDatabase.BuildingDefination[] buildings)
	{
		EventManager.InvokeEvent(GameEvent.OnBuildWindowOpen);
		if (_state == BuildState.HQPlacement)
			return;
		GameRegistry.InteractionUI.interactionPanel.HidePanel();
		buildWindow.SetActive(true);
		_state = BuildState.Idle;
		_selectedBuilding = null;
		if (_activeUnits.Count < buildings.Length)
		{
			GrowUnitsUI(buildings.Length);
		}
		var activeCount = 0;
		for (int i = 0; i < buildings.Length; i++)
		{
			if (!GameRegistry.IsBuildingUnlocked(buildings[i].id))
			{
				_activeUnits[i].gameObject.SetActive(false);
				continue;
			}
			var building = buildings[i].info;
#if DEBUG
			if (building == null)
			{
				Debug.LogWarning("Null building in list, aborting");
				break;
			}
#endif
			_activeUnits[i].gameObject.SetActive(true);
			activeCount++;
			_activeUnits[i].text.SetText(building.name);
			_activeUnits[i].ClearAllEvents();
			_activeUnits[i].OnClick += () =>
			{
				if (GameRegistry.ResourceSystem.HasAllResources(building.cost))
				{
					_selectedBuilding = building;
					_state = BuildState.Placement;
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
		EventManager.InvokeEvent(GameEvent.OnBuildWindowClose);
		HideAllIndicators();
		_state = BuildState.Disabled;
		_lastBuildingCategory = null;
		_selectedBuilding = null;
		buildWindow.SetActive(false);
		for (int i = 0; i < _activeUnits.Count; i++)
		{
			_activeUnits[i]?.gameObject.SetActive(false);
		}
	}
}