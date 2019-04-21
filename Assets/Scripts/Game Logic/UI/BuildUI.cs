﻿using System.Collections;
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

	public MobileUnitInfo testUnit;

	/*	UI	*/
	public UIInfoBanner infoBanner;
	public BaseNameWindowUI baseNameUI;
	public TMP_Text baseNameText;
	public RectTransform buildWindow;
	//Indicators
	public MeshEntity selectIndicatorEntity;
	public MeshEntity placementPathIndicatorEntity;
	public MeshEntity errorIndicatorEntity;
	//Tooltip
	public UITooltip toolTip;
	//State
	[HideInInspector]
	public bool placeMode;
	[HideInInspector]
	public bool hqMode;

	public RectTransform unitUIPrefab;
	public bool uiBlock;

	private UIUnitIcon[] _activeUnits;
	private BuildingTileInfo _selectedUnit;
	private Camera _cam;
	private Dictionary<MeshEntity, List<Entity>> _indicatorEntities;
	private EntityManager _EM;

	private Tile _startPoint;
	private List<Tile> _buildPath;

	void Start()
	{
		buildWindow.gameObject.SetActive(false);
		_indicatorEntities = new Dictionary<MeshEntity, List<Entity>>();
		_activeUnits = new UIUnitIcon[6];
		_cam = Camera.main;
		_EM = World.Active.EntityManager;
        HideBuildWindow();
		placeMode = hqMode = true;
		_selectedUnit = HQTile;
		toolTip.HideToolTip();
		infoBanner.SetText("Place HQ Building");
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			if (placeMode && !hqMode)
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
					var tilesToOccupy = Map.ActiveMap.HexSelect(selectedTile.Coords, _selectedUnit.size);
					var validPlacement = false;
					HideAllIndicators();
					//Path Placement
					if (!hqMode && Input.GetKey(KeyCode.Mouse0) && _startPoint != null)
					{
						_buildPath = Map.ActiveMap.GetPath(_startPoint, selectedTile);
						if (_buildPath != null)
						{
							if(_buildPath.Any(t => t.Height <= Map.ActiveMap.SeaLevel))
							{
								var invalidTiles = _buildPath.Where(t => t.Height <= Map.ActiveMap.SeaLevel);
								ShowIndicators(errorIndicatorEntity, invalidTiles.ToList());
								ShowIndicators(selectIndicatorEntity, _buildPath.Except(invalidTiles).ToList());
							}
							else
								ShowIndicators(placementPathIndicatorEntity, _buildPath);
						}
					}
					if (!hqMode && Input.GetKeyDown(KeyCode.Mouse0))
					{
						_startPoint = selectedTile;
					}
					//Indicators
					if(!ResourceSystem.HasResourses(_selectedUnit.cost))
					{
						ShowIndicators(errorIndicatorEntity, tilesToOccupy);
					}
					else if(tilesToOccupy.Any(t => t.Height <= Map.ActiveMap.SeaLevel))
					{
						var invalidTiles = tilesToOccupy.Where(t => t.Height <= Map.ActiveMap.SeaLevel);
						ShowIndicators(errorIndicatorEntity, invalidTiles.ToList());
						ShowIndicators(selectIndicatorEntity, tilesToOccupy.Except(invalidTiles).ToList());
					}
					else if(tilesToOccupy.Any(t => t is BuildingTile))
					{
						var invalidTiles = tilesToOccupy.Where(t => t is BuildingTile);
						ShowIndicators(errorIndicatorEntity, invalidTiles.ToList());
						ShowIndicators(selectIndicatorEntity, tilesToOccupy.Except(invalidTiles).ToList());
					}
					else
					{
						ShowIndicators(selectIndicatorEntity, tilesToOccupy);
						validPlacement = true;
					}

					//Placement
					if (Input.GetKeyUp(KeyCode.Mouse0))
					{
						_startPoint = null;
						if (validPlacement)
						{
							if (!hqMode)
								ResourceSystem.ConsumeResourses(_selectedUnit.cost);
							if (_buildPath == null)
							{
								Map.ActiveMap.HexFlatten(selectedTile.Coords, _selectedUnit.size, _selectedUnit.influenceRange, Map.FlattenMode.Average);
								Map.ActiveMap.ReplaceTile(selectedTile, _selectedUnit);
							}else
							{
								for (int i = 0; i < _buildPath.Count; i++)
								{
									if (_buildPath[i].Height <= Map.ActiveMap.SeaLevel)
										continue;
									if (_buildPath[i] is BuildingTile)
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
									testUnit.Instantiate(selectedTile.SurfacePoint, Quaternion.identity);
								};
							}
						}
						HideAllIndicators();
					}
				}
				else
					HideAllIndicators();
			}
		}
	}

	void HideIndicator(MeshEntity indicator)
	{
		for (int i = 0; i < _indicatorEntities[indicator].Count; i++)
		{
			if (!_EM.HasComponent<Frozen>(_indicatorEntities[indicator][i]))
				_EM.AddComponent(_indicatorEntities[indicator][i], typeof(Frozen));
		}
	}

	void HideAllIndicators()
	{
		foreach (var indicators in _indicatorEntities)
		{
			HideIndicator(indicators.Key);
		}
	}

	private void OnDestroy()
	{
	}

	void HideIndicators(List<Entity> entities)
	{
		for (int i = 0; i < entities.Count; i++)
		{
			if (!_EM.HasComponent<Frozen>(entities[i]))
				_EM.AddComponent(entities[i], typeof(Frozen));
		}
	}

	void GrowIndicators(MeshEntity indicatorMesh, int count)
	{
		List<Entity> entities;
		if (!_indicatorEntities.ContainsKey(indicatorMesh))
		{
			_indicatorEntities.Add(indicatorMesh, entities = new List<Entity>());
		}else
		{
			entities = _indicatorEntities[indicatorMesh];
			if (count <= entities.Count)
				return;
		}
		for (int i = 0; i < count; i++)
		{
			entities.Add(indicatorMesh.Instantiate(Vector3.zero, Vector3.one * .9f));
			Map.EM.AddComponent(entities[i], typeof(Frozen));
		}
	}

	void ShowIndicators(MeshEntity indicatorMesh, List<Tile> tiles)
	{
		GrowIndicators(indicatorMesh, tiles.Count);
		for (int i = 0; i < _indicatorEntities[indicatorMesh].Count; i++)
		{
			if (i < tiles.Count)
			{
				if (_EM.HasComponent<Frozen>(_indicatorEntities[indicatorMesh][i]))
					_EM.RemoveComponent<Frozen>(_indicatorEntities[indicatorMesh][i]);
				_EM.SetComponentData(_indicatorEntities[indicatorMesh][i], new Translation { Value = tiles[i].SurfacePoint });
			}
			else
			{
				if (!_EM.HasComponent<Frozen>(_indicatorEntities[indicatorMesh][i]))
					_EM.AddComponent(_indicatorEntities[indicatorMesh][i], typeof(Frozen));
			}
		}
	}


	public void ShowBuildWindow(BuildingTileInfo[] units)
	{
		if (hqMode)
			return;
		HideBuildWindow();
		buildWindow.gameObject.SetActive(true);
		for (int i = 0; i < units.Length; i++)
		{
			var unit = units[i];
			if(_activeUnits[i] == null)
			{
				_activeUnits[i] = Instantiate(unitUIPrefab, buildWindow).GetComponent<UIUnitIcon>();
				_activeUnits[i].anchoredPosition = new Vector2(5 + (i * 170), 5);
			}
			_activeUnits[i]?.gameObject.SetActive(true);
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
		buildWindow.gameObject.SetActive(false);
		for (int i = 0; i < _activeUnits.Length; i++)
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
