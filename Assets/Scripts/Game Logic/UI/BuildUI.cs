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
	public UnitInfo[] Tech;
	public UnitInfo[] Resource;
	public UnitInfo[] Economy;
	public UnitInfo[] Structure;
	public UnitInfo[] Millitary;
	public UnitInfo[] Defense;
	public UnitInfo HQUnit;

	public MobileUnitInfo testUnit;

	/*	UI	*/
	public UIInfoBanner infoBanner;
	public BaseNameWindowUI baseNameUI;
	public TMP_Text baseNameText;
	//Indicators
	public MeshEntity selectIndicatorEntity;
	public MeshEntity powerIndicatorEntity;
	public RectTransform buildWindow;
	//Tooltip
	public UITooltip toolTip;
	//State
	public bool placeMode;

	public RectTransform unitUIPrefab;
	public bool uiBlock;

	private UIUnitIcon[] _activeUnits;
	private UnitInfo _selectedUnit;
	private bool _hqMode;
	private Camera _cam;
	private NativeArray<Entity> _selectIndicatorEntities;
	private NativeArray<Entity> _powerIndicatorEntities;
	private EntityManager _EM;

	private Tile _startPoint;
	private List<Tile> _buildPath;

	void Start()
	{
		buildWindow.gameObject.SetActive(false);
		_activeUnits = new UIUnitIcon[6];
		_cam = Camera.main;
		_selectIndicatorEntities = new NativeArray<Entity>(0, Allocator.Persistent);
		_powerIndicatorEntities = new NativeArray<Entity>(0, Allocator.Persistent);
		_EM = World.Active.EntityManager;
        HideBuildWindow();
		placeMode = _hqMode = true;
		_selectedUnit = HQUnit;
		toolTip.HideToolTip();
		infoBanner.SetText("Place HQ Building");
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			if (placeMode && !_hqMode)
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
				if (selectedTile != null && selectedTile.Height > Map.ActiveMap.SeaLevel)
				{
					var tilesToOccupy = Map.ActiveMap.HexSelect(selectedTile.Coords, _selectedUnit.tile.size);
					ShowIndicators(ref _selectIndicatorEntities, selectIndicatorEntity, tilesToOccupy);
					if(!_hqMode && Input.GetKeyDown(KeyCode.Mouse0))
					{
						_startPoint = selectedTile;
					}
					if (!_hqMode && Input.GetKey(KeyCode.Mouse0) && _startPoint != null)
					{
						_buildPath = Map.ActiveMap.GetPath(_startPoint, selectedTile);
						if (_buildPath != null)
							ShowIndicators(ref _powerIndicatorEntities, powerIndicatorEntity, _buildPath);
					}
					if (Input.GetKeyUp(KeyCode.Mouse0))
					{
						_startPoint = null;
						if (_hqMode || _buildPath != null || (!tilesToOccupy.Any(t => t is BuildingTile)))
						{
							if (!_hqMode)
								ConsumeResourse();
							if (_buildPath == null)
							{
								Map.ActiveMap.HexFlatten(selectedTile.Coords, _selectedUnit.tile.size, _selectedUnit.tile.influenceRange, Map.FlattenMode.Average);
								Map.ActiveMap.ReplaceTile(selectedTile, _selectedUnit.tile);
							}else
							{
								for (int i = 0; i < _buildPath.Count; i++)
								{
									if (_buildPath[i] is BuildingTile)
										continue;
									Map.ActiveMap.HexFlatten(_buildPath[i].Coords, _selectedUnit.tile.size, _selectedUnit.tile.influenceRange, Map.FlattenMode.Average);
									Map.ActiveMap.ReplaceTile(_buildPath[i], _selectedUnit.tile);
								}
							}
							if (_hqMode)
							{
								baseNameUI.Show();
								placeMode = false;
								_cam.GetComponent<CameraController>().enabled = false;
								baseNameUI.OnHide += () =>
								{
									placeMode = _hqMode = false;
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

	void ConsumeResourse()
	{
		for (int i = 0; i < _selectedUnit.cost.Length; i++)
		{
			var cost = _selectedUnit.cost[i];
			ResourceSystem.resCount[ResourceDatabase.GetResourceId(cost.name)] -= cost.ammount;
		}

	}

	bool HasResourse(UnitInfo.Resource[] resources)
	{
		for (int i = 0; i < resources.Length; i++)
		{
			var id = ResourceDatabase.GetResourceId(resources[i].name);
			if (ResourceSystem.resCount[id] < resources[i].ammount)
				return false;
		}
		return true;
	}

	void HideAllIndicators()
	{
		HideIndicators(_selectIndicatorEntities);
		HideIndicators(_powerIndicatorEntities);
	}

	private void OnDestroy()
	{
		_selectIndicatorEntities.Dispose();
		_powerIndicatorEntities.Dispose();
	}

	void HideIndicators(NativeArray<Entity> entities)
	{
		for (int i = 0; i < entities.Length; i++)
		{
			if (!_EM.HasComponent<Frozen>(entities[i]))
				_EM.AddComponent(entities[i], typeof(Frozen));
		}
	}

	void GrowIndicators(ref NativeArray<Entity> entities, MeshEntity meshEntity, int count)
	{
		if (count <= entities.Length)
			return;
		var newEntities = new NativeArray<Entity>(count, Allocator.Persistent);
		for (int i = 0; i < count; i++)
		{
			if (i < entities.Length)
			{
				newEntities[i] = entities[i];
				continue;
			}
			newEntities[i] = meshEntity.Instantiate(Vector3.zero, Vector3.one * .9f);
			Map.EM.AddComponent(newEntities[i], typeof(Frozen));
		}
		entities.Dispose();
		entities = newEntities;
	}

	void ShowIndicators(ref NativeArray<Entity> indicators, MeshEntity baseIndicator, List<Tile> tiles)
	{
		if (indicators.Length < tiles.Count)
		{
			GrowIndicators(ref indicators, baseIndicator, tiles.Count);
		}
		for (int i = 0; i < indicators.Length; i++)
		{
			if (i < tiles.Count)
			{
				if (_EM.HasComponent<Frozen>(indicators[i]))
					_EM.RemoveComponent<Frozen>(indicators[i]);
				_EM.SetComponentData(indicators[i], new Translation { Value = tiles[i].SurfacePoint });
			}
			else
			{
				if (!_EM.HasComponent<Frozen>(indicators[i]))
					_EM.AddComponent(indicators[i], typeof(Frozen));
			}
		}
	}


	public void ShowBuildWindow(UnitInfo[] units)
	{
		if (_hqMode)
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
				if(HasResourse(unit.cost))
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
