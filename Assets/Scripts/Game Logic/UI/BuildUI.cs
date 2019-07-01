using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using System.Collections.Concurrent;
using Unity.Rendering;
using System.Linq;
using UnityEngine.EventSystems;
using AnimationSystem.Animations;
using AnimationSystem.AnimationData;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class BuildUI : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
	public BuildingDatabase buildings;
	public HQTileInfo HQTile;
	public MeshEntityRotatable landingMesh;

	/*	UI	*/
	public UIInfoBanner infoBanner;
	public GameObject buildWindow;
	public RectTransform scrollContent;
	//Indicators
	public MeshEntity selectIndicatorEntity;
	public MeshEntity placementPathIndicatorEntity;
	public MeshEntity gatheringIndicatorEntity;
	public MeshEntity errorIndicatorEntity;
	//Tooltip
	public UITooltip toolTip;
	public TMP_Text floatingText;
	//State
	//[HideInInspector]
	public bool placeMode;
	//[HideInInspector]
	public bool hqMode;

	public RectTransform unitUIPrefab;
	public bool uiBlock;

	private List<UIUnitIcon> _activeUnits;
	private BuildingTileInfo _selectedBuilding;
	private Camera _cam;
	private Dictionary<MeshEntity, List<Entity>> _indicatorEntities;
	private Dictionary<MeshEntity, int> _renderedEntities;
	private EntityManager _EM;

	private Tile _startPoint;
	private List<Tile> _buildPath;
	private System.Func<Tile, bool> invalidTileSelector;
	private bool _validPlacement;
	private BuildingDatabase.BuildingDefination[] _lastBuildingList;
	private Dictionary<int, BuildOrder> _pendingBuildOrders;
	private List<int> _readyToBuildOrders;

	struct BuildOrder
	{
		public Tile dstTile;
		public BuildingTileInfo building;
	}

	void Awake()
	{
		GameRegistry.INST.buildUI = this;
		GameRegistry.INST.buildingDatabase = buildings;
	}

	void Start()
	{
		_pendingBuildOrders = new Dictionary<int, BuildOrder>();
		_readyToBuildOrders = new List<int>();
		_indicatorEntities = new Dictionary<MeshEntity, List<Entity>>();
		_renderedEntities = new Dictionary<MeshEntity, int>();
		_activeUnits = new List<UIUnitIcon>();
		_cam = GameRegistry.Camera;
		_EM = World.Active.EntityManager;
        HideBuildWindow();
		placeMode = hqMode = true;
		_selectedBuilding = HQTile;
		toolTip.HideToolTip();
		infoBanner.SetText("Place HQ Building");
		invalidTileSelector = t =>
			_pendingBuildOrders.Values.Any(o => o.dstTile == t) ||
			t.Height <= Map.ActiveMap.seaLevel ||
			t is BuildingTile ||
			t is ResourceTile;
		if(selectIndicatorEntity.mesh == null || selectIndicatorEntity.material == null)
			Debug.LogError("Null");

		EventManager.AddEventListener("buildingUnlocked", () =>
		{
			if(_lastBuildingList != null)
				ShowBuildWindow(_lastBuildingList);
		});
	}

	// Update is called once per frame
	void Update()
	{
		BuildReadyBuildings();
		if (Input.GetKey(KeyCode.Escape) && !hqMode)
		{
			if (placeMode)
			{
				placeMode = false;
				HideAllIndicators();
			}
			else
				HideBuildWindow();
		}
		var mPos = Input.mousePosition;
		if (!uiBlock)
		{
			var selectedTile = Map.ActiveMap.GetTileFromRay(_cam.ScreenPointToRay(mPos), _cam.transform.position.y * 2);
			if (placeMode)
			{
				if (selectedTile != null)
				{
					_validPlacement = true;

					var tilesToOccupy = Map.ActiveMap.HexSelect(selectedTile.Coords, _selectedBuilding.size);
					//Path Placement
					if (_selectedBuilding.placementMode == PlacementMode.Path && Input.GetKeyDown(KeyCode.Mouse0))
					{
						_startPoint = selectedTile;
					}
					if (_selectedBuilding.placementMode == PlacementMode.Path && Input.GetKey(KeyCode.Mouse0) && _startPoint != null)
					{
						if (invalidTileSelector(selectedTile))
						{
							HideIndicator(placementPathIndicatorEntity);
							_buildPath = null;
						}
						if (!(selectedTile is ResourceTile))
							_buildPath = Map.ActiveMap.GetPath(_startPoint, selectedTile, filter: t => !(t is ResourceTile));
						else
							_buildPath = null;
						if (_buildPath != null)
						{
							if (_buildPath.Any(t => t.Height <= Map.ActiveMap.seaLevel))
							{
								var invalidTiles = _buildPath.Where(t => t.Height <= Map.ActiveMap.seaLevel);
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
					//Indicators
					if (!ResourceSystem.HasResourses(_selectedBuilding.cost))
					{
						HideIndicator(selectIndicatorEntity);
						ShowIndicators(errorIndicatorEntity, tilesToOccupy);
						_validPlacement = false;
					}
					else if (tilesToOccupy.Any(invalidTileSelector))
					{
						var invalidTiles = tilesToOccupy.Where(invalidTileSelector);
						ShowIndicators(errorIndicatorEntity, invalidTiles.ToList());
						ShowIndicators(selectIndicatorEntity, tilesToOccupy.Except(invalidTiles).ToList());
						_validPlacement = false;
					}
					else
					{
						HideIndicator(errorIndicatorEntity);
						ShowIndicators(selectIndicatorEntity, tilesToOccupy);
						_validPlacement = true;
					}
					if (_selectedBuilding is ResourceGatheringBuildingInfo r)
					{
						var res = Map.ActiveMap.HexSelect(selectedTile.Coords, r.gatherRange, true) //Select Tiles
								.Where(t => t is ResourceTile rt && !rt.gatherer.isCreated) //Exclude Tiles that are not resource tiles or being gathered already
								.Where(rt => r.resourcesToGather.Any(rG => ResourceDatabase.GetResourceTile(rG.id) == rt.info)).ToList();
						if (res.Count > 0)
						{
							_validPlacement = true;
							var resCount = new Dictionary<int, int>();
							for (int i = 0; i < res.Count; i++)
							{
								var id = ResourceDatabase.GetResourceId(res[i].info as ResourceTileInfo);
								if (resCount.ContainsKey(id))
									resCount[id]++;
								else
									resCount.Add(id, 1);
							}
							var sb = new System.Text.StringBuilder();
							for (int i = 0; i < r.resourcesToGather.Length; i++)
							{
								var rG = r.resourcesToGather[i];
								if (resCount.ContainsKey(rG.id))
								{
									var gatherAmmount = (int)(resCount[rG.id] * rG.ammount);
									if(gatherAmmount > 0)
										sb.AppendLine($"+{gatherAmmount}{ResourceDatabase.GetResourceString(rG.id)}");
								}

							}
							floatingText.SetText(sb);
							var pos = _cam.WorldToScreenPoint(selectedTile.SurfacePoint);
							pos.y += 20;
							floatingText.rectTransform.position = pos;
							floatingText.gameObject.SetActive(true);
							ShowIndicators(gatheringIndicatorEntity, res);
							if (!invalidTileSelector(selectedTile))
								HideIndicator(errorIndicatorEntity);
						}
						else
						{
							_validPlacement = false;
							HideIndicator(gatheringIndicatorEntity);
							HideIndicator(selectIndicatorEntity);
							ShowIndicators(errorIndicatorEntity, tilesToOccupy);
							floatingText.gameObject.SetActive(false);
						}
					}
					//Placement
					if (Input.GetKeyUp(KeyCode.Mouse0))
					{
						_startPoint = null;
						if (_validPlacement || _selectedBuilding.placementMode == PlacementMode.Path)
						{
							HideAllIndicators();
							if (!hqMode)
								ResourceSystem.ConsumeResourses(_selectedBuilding.cost);
							if (_buildPath == null)
							{
								QueueBuilding(selectedTile);
							}
							else
							{
								for (int i = 0; i < _buildPath.Count; i++)
								{
									if (invalidTileSelector(_buildPath[i]))
										continue;
									QueueBuilding(_buildPath[i]);
								}
								_buildPath = null;
							}
							if (hqMode)
							{
								placeMode = false;
								infoBanner.SetActive(false);
								void onHide()
								{
									placeMode = hqMode = false;
									GameRegistry.BaseNameUI.OnHide -= onHide;
								}
								GameRegistry.BaseNameUI.OnHide += onHide;
							}
						}
					}
				}
				else
					HideAllIndicators();
			}
		}
	}

	void QueueBuilding(Tile tile)
	{
		var pos = tile.SurfacePoint;
		pos.y = Random.Range(90, 100);
		var e = landingMesh.Instantiate(pos);
		_EM.AddComponentData(e, new FallAnim
		{
			startSpeed = new float3(0,Random.Range(-100, -90),0)
		}) ; 
		_EM.AddComponentData(e, new Floor
		{
			Value = tile.Height
		});
		var callback = tile.Coords.GetHashCode();
		_pendingBuildOrders.Add(callback, new BuildOrder
		{
			building = _selectedBuilding,
			dstTile = tile
		});
		if (hqMode)
		{
			EventManager.AddEventListener(callback.ToString(), () =>
			{
				_readyToBuildOrders.Add(callback);
				GameRegistry.BaseNameUI.Show();
			});
		}
		else
		{
			EventManager.AddEventListener(callback.ToString(), () =>
			{
				_readyToBuildOrders.Add(callback);
			});
		}
		_EM.AddComponentData(e, new HitFloorCallback
		{
			eventId = callback
		});
		_EM.AddComponentData(e, new Gravity { Value = 9.8f });
	}

	void BuildReadyBuildings()
	{
		for (int i = 0; i < _readyToBuildOrders.Count; i++)
		{
			var orderId = _readyToBuildOrders[i];
			EventManager.RemoveAllEventListeners(orderId.ToString());
			PlaceBuilding(_pendingBuildOrders[orderId]);
			_pendingBuildOrders.Remove(orderId);
		}
		_readyToBuildOrders.Clear();
	}

	void PlaceBuilding(BuildOrder order)
	{
		Map.ActiveMap.HexFlatten(order.dstTile.Coords, order.building.size, order.building.flattenOuterRange, Map.FlattenMode.Average);
		Map.ActiveMap.ReplaceTile(order.dstTile, order.building);
	}

	void HideIndicator(MeshEntity indicator)
	{
		if (!_indicatorEntities.ContainsKey(indicator))
			return;
		for (int i = 0; i < _renderedEntities[indicator]; i++)
		{
			_EM.AddComponent(_indicatorEntities[indicator][i], typeof(Disabled));
		}
		_renderedEntities[indicator] = 0;
	}

	void HideAllIndicators()
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

	void GrowIndicators(MeshEntity indicatorMesh, int count)
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
			Map.EM.AddComponent(curEntity, typeof(Disabled));
		}
	}

	void ShowIndicators(MeshEntity indicatorMesh, List<Tile> tiles)
	{
		GrowIndicators(indicatorMesh, tiles.Count);
		for (int i = 0; i < _indicatorEntities[indicatorMesh].Count; i++)
		{
			if (i < tiles.Count)
			{
				if(i >= _renderedEntities[indicatorMesh])
					_EM.RemoveComponent<Disabled>(_indicatorEntities[indicatorMesh][i]);

				_EM.SetComponentData(_indicatorEntities[indicatorMesh][i], new Translation { Value = tiles[i].SurfacePoint });
			}
			else
			{
				if (i >= _renderedEntities[indicatorMesh])
					break;
				if(i < _renderedEntities[indicatorMesh])
					_EM.AddComponent(_indicatorEntities[indicatorMesh][i], typeof(Disabled));
			}
		}
		_renderedEntities[indicatorMesh] = tiles.Count;
	}


	public void ShowBuildWindow(BuildingDatabase.BuildingDefination[] buildings)
	{
		if (hqMode)
			return;
		_lastBuildingList = buildings;
		buildWindow.SetActive(true);
		if(_activeUnits.Count < buildings.Length)
		{
			GrowUnitsUI(buildings.Length);
		}
		var activeCount = 0;
		for (int i = 0; i < buildings.Length; i++)
		{
			if (!buildings[i].isUnlocked)
			{
				_activeUnits[i].gameObject.SetActive(false);
				continue;
			}
			var building = buildings[i].info;
#if DEBUG
			if(building == null)
			{
				Debug.LogWarning("Null building in list, aborting");
				break;
			}
#endif
			_activeUnits[i].gameObject.SetActive(true);
			_activeUnits[i].text.SetText(building.name);
			_activeUnits[i].ClearAllEvents();
			_activeUnits[i].OnClick += () =>
			{
				Debug.Log(building.name);
				if(ResourceSystem.HasResourses(building.cost))
				{
					_selectedBuilding = building;
					placeMode = true;
				}
			};
			_activeUnits[i].OnHover += () => toolTip.ShowToolTip(building.name, building.description, building.GetCostString(), building.GetProductionString());
			_activeUnits[i].OnBlur += () => toolTip.HideToolTip();
			_activeUnits[i].icon.sprite = building.icon;
		}
		for (int i = buildings.Length; i < _activeUnits.Count; i++)
		{
			_activeUnits[i].gameObject.SetActive(false);
		}
		scrollContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 5 + (activeCount * 170));
	}

	public void GrowUnitsUI(int count)
	{
		for (int i = _activeUnits.Count; i < count; i++)
		{
			var unit = Instantiate(unitUIPrefab, scrollContent).GetComponent<UIUnitIcon>();
			unit.anchoredPosition = new Vector2(5 + (i * 170), 5);
			_activeUnits.Add(unit);
		}
	}

	public void ShowTechWindow() => ShowBuildWindow(buildings[BuildingCategory.Tech]);
	public void ShowResourcesWindow() => ShowBuildWindow(buildings[BuildingCategory.Resources]);
	public void ShowEcoWindow() => ShowBuildWindow(buildings[BuildingCategory.Economy]);
	public void ShowStructureWindow() => ShowBuildWindow(buildings[BuildingCategory.Structure]);
	public void ShowMilitaryWindow() => ShowBuildWindow(buildings[BuildingCategory.Military]);
	public void ShowDefenseWindow() => ShowBuildWindow(buildings[BuildingCategory.Defense]);

	public void HideBuildWindow()
	{
		HideAllIndicators();
		placeMode = false;
		_lastBuildingList = null;
		_selectedBuilding = null;
		buildWindow.SetActive(false);
		for (int i = 0; i < _activeUnits.Count; i++)
		{
			_activeUnits[i]?.gameObject.SetActive(false);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		HideAllIndicators();
		uiBlock = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		uiBlock = false;
	}
}
