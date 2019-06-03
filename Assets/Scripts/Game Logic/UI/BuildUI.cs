using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using System.Linq;
using UnityEngine.EventSystems;

public class BuildUI : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
	public BuildingTileInfo[] Tech;
	public BuildingTileInfo[] Resource;
	public BuildingTileInfo[] Economy;
	public BuildingTileInfo[] Structure;
	public BuildingTileInfo[] Millitary;
	public BuildingTileInfo[] Defense;
	public HQTileInfo HQTile;

	/*	UI	*/
	public UIInfoBanner infoBanner;
	public BaseNameWindowUI baseNameUI;
	public TMP_Text baseNameText;
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
	[HideInInspector]
	public bool placeMode;
	[HideInInspector]
	public bool hqMode;

	public RectTransform unitUIPrefab;
	public bool uiBlock;

	private List<UIUnitIcon> _activeUnits;
	private BuildingTileInfo _selectedUnit;
	private Camera _cam;
	private Dictionary<MeshEntity, List<Entity>> _indicatorEntities;
	private Dictionary<MeshEntity, int> _renderedEntities;
	private EntityManager _EM;

	private Tile _startPoint;
	private List<Tile> _buildPath;
	private System.Func<Tile, bool> invalidTileSelector;
	private Tile _lastSelectedTile = null;
	private bool _validPlacement;

	void Start()
	{
		buildWindow.SetActive(false);
		_indicatorEntities = new Dictionary<MeshEntity, List<Entity>>();
		_renderedEntities = new Dictionary<MeshEntity, int>();
		_activeUnits = new List<UIUnitIcon>();
		_cam = Camera.main;
		_EM = World.Active.EntityManager;
        HideBuildWindow();
		placeMode = hqMode = true;
		_selectedUnit = HQTile;
		toolTip.HideToolTip();
		infoBanner.SetText("Place HQ Building");
		invalidTileSelector = t =>
			t.Height <= Map.ActiveMap.seaLevel ||
			t is BuildingTile ||
			t is ResourceTile;
	}

	// Update is called once per frame
	void Update()
	{
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
					if (_lastSelectedTile != selectedTile)
					{
						_validPlacement = true;
						_lastSelectedTile = selectedTile;

						var tilesToOccupy = Map.ActiveMap.HexSelect(selectedTile.Coords, _selectedUnit.size);
						//Path Placement
						if (_selectedUnit.placementMode == PlacementMode.Path && Input.GetKey(KeyCode.Mouse0) && _startPoint != null)
						{
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
						if (_selectedUnit.placementMode == PlacementMode.Path && Input.GetKeyDown(KeyCode.Mouse0))
						{
							_startPoint = selectedTile;
						}
						//Indicators
						if (!ResourceSystem.HasResourses(_selectedUnit.cost))
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
						if (_selectedUnit is ResourceGatheringBuildingInfo r)
						{
							var res = Map.ActiveMap.HexSelect(selectedTile.Coords, r.gatherRange, true).Where(t => t is ResourceTile rt && !rt.gatherer.isCreated).Where(rt => r.resourcesToGather.Any(rG => ResourceDatabase.GetResourceTile(rG.id) == rt.info)).ToList();
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
								foreach (var resItem in resCount)
								{
									if (resItem.Value > 0)
										sb.AppendLine($"+{resItem.Value}<sprite={ResourceDatabase.GetSpriteId(resItem.Key)}>");
								}
								floatingText.SetText(sb);
								var pos = _cam.WorldToScreenPoint(selectedTile.SurfacePoint);
								pos.y += 20;
								floatingText.rectTransform.position = pos;
								floatingText.gameObject.SetActive(true);
								ShowIndicators(gatheringIndicatorEntity, res);
								HideIndicator(errorIndicatorEntity);
							}
							else
							{
								_validPlacement = false;
								HideIndicator(gatheringIndicatorEntity);
								ShowIndicators(errorIndicatorEntity, tilesToOccupy);
							}
						}
					}
					//Placement
					if (Input.GetKeyUp(KeyCode.Mouse0))
					{
						_startPoint = null;
						if (_validPlacement)
						{
							HideAllIndicators();
							if (!hqMode)
								ResourceSystem.ConsumeResourses(_selectedUnit.cost);
							if (_buildPath == null)
							{
								Map.ActiveMap.HexFlatten(selectedTile.Coords, _selectedUnit.size, _selectedUnit.influenceRange, Map.FlattenMode.Average);
								Map.ActiveMap.ReplaceTile(selectedTile, _selectedUnit);
							}
							else
							{
								for (int i = 0; i < _buildPath.Count; i++)
								{
									if (invalidTileSelector(_buildPath[i]))
										continue;
									Map.ActiveMap.HexFlatten(_buildPath[i].Coords, _selectedUnit.size, _selectedUnit.influenceRange, Map.FlattenMode.Average);
									Map.ActiveMap.ReplaceTile(_buildPath[i], _selectedUnit);
								}
							}
							if (hqMode)
							{
								baseNameUI.Show();
								placeMode = false;
								_cam.GetComponent<CameraController>().enabled = false;
								baseNameUI.OnHide += () =>
								{
									placeMode = hqMode = false;
									infoBanner.SetActive(false);
									_cam.GetComponent<CameraController>().enabled = true;
									baseNameText.text = baseNameUI.text.text;
								};
							}
						}
					}
				}
				else
					HideAllIndicators();
			}
		}
	}

	void HideIndicator(MeshEntity indicator)
	{
		if (!_indicatorEntities.ContainsKey(indicator))
			return;
		for (int i = 0; i < _renderedEntities[indicator]; i++)
		{
			_EM.AddComponent(_indicatorEntities[indicator][i], typeof(FrozenRenderSceneTag));
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
		if(Map.ActiveMap.IsRendered)
			HideAllIndicators();
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
			Map.EM.AddComponent(curEntity, typeof(FrozenRenderSceneTag));
		}
	}

	void ShowIndicators(MeshEntity indicatorMesh, List<Tile> tiles)
	{
		GrowIndicators(indicatorMesh, tiles.Count);
		for (int i = 0; i < _indicatorEntities[indicatorMesh].Count; i++)
		{
			if (i < tiles.Count)
			{
				if(i < _renderedEntities[indicatorMesh])
					_EM.RemoveComponent<FrozenRenderSceneTag>(_indicatorEntities[indicatorMesh][i]);

				_EM.SetComponentData(_indicatorEntities[indicatorMesh][i], new Translation { Value = tiles[i].SurfacePoint });
			}
			else
			{
				if (i >= _renderedEntities[indicatorMesh])
					break;
				if(i < _renderedEntities[indicatorMesh])
					_EM.AddComponent(_indicatorEntities[indicatorMesh][i], typeof(FrozenRenderSceneTag));
			}
		}
		_renderedEntities[indicatorMesh] = tiles.Count;
	}


	public void ShowBuildWindow(BuildingTileInfo[] units)
	{
		if (hqMode)
			return;
		HideBuildWindow();
		buildWindow.SetActive(true);
		if(_activeUnits.Count < units.Length)
		{
			GrowUnitsUI(units.Length);
		}
		scrollContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 5 + (units.Length * 170));
		for (int i = 0; i < units.Length; i++)
		{
			var unit = units[i];
#if DEBUG
			if(unit == null)
			{
				Debug.LogWarning("Null unit in list, aborting");
				break;
			}
#endif
			_activeUnits[i].gameObject.SetActive(true);
			_activeUnits[i].text.SetText(unit.name);
			_activeUnits[i].OnClick += () =>
			{
				if(ResourceSystem.HasResourses(unit.cost))
				{
					_selectedUnit = unit;
					placeMode = true;
				}
			};
			_activeUnits[i].OnHover += () => toolTip.ShowToolTip(unit.name, unit.description, unit.GetCostString(), unit.GetProductionString());
			_activeUnits[i].OnBlur += () => toolTip.HideToolTip();
			_activeUnits[i].icon.sprite = unit.icon;
		}
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

	public void ShowTechWindow() => ShowBuildWindow(Tech);
	public void ShowResourcesWindow() => ShowBuildWindow(Resource);
	public void ShowEcoWindow() => ShowBuildWindow(Economy);
	public void ShowStructureWindow() => ShowBuildWindow(Structure);
	public void ShowMilitaryWindow() => ShowBuildWindow(Millitary);
	public void ShowDefenseWindow() => ShowBuildWindow(Defense);

	public void HideBuildWindow()
	{
		HideAllIndicators();
		placeMode = false;
		_selectedUnit = null;
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
