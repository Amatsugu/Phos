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

public class BuildUI : MonoBehaviour
{
	public UnitInfo[] Tech;
	public UnitInfo[] Resource;
	public UnitInfo[] Economy;
	public UnitInfo[] Structure;
	public UnitInfo[] Millitary;
	public UnitInfo[] Defense;
	public UnitInfo HQUnit;

	/*	UI	*/
	public UIInfoBanner InfoBanner;
	//Indicators
	public MeshEntity selectIndicatorEntity;
	public MeshEntity powerIndicatorEntity;
	public RectTransform buildWindow;
	//Tooltip
	public UITooltip toolTip;

	public RectTransform unitUIPrefab;

	private UIUnitIcon[] _activeUnits;
	private UnitInfo _selectedUnit;
	private bool _placeMode;
	private bool _hqMode;
	private Camera _cam;
	private bool _toolTipVisible;
	private NativeArray<Entity> _selectIndicatorEntities;
	private NativeArray<Entity> _powerIndicatorEntities;
	private EntityManager _EM;
	private Rect _buildBarRect;

	void Start()
	{
		buildWindow.gameObject.SetActive(false);
		_activeUnits = new UIUnitIcon[6];
		_cam = Camera.main;
		_selectIndicatorEntities = new NativeArray<Entity>(0, Allocator.Persistent);
		_powerIndicatorEntities = new NativeArray<Entity>(0, Allocator.Persistent);
		_EM = World.Active.EntityManager;
		_buildBarRect = new Rect
		{
			x = buildWindow.position.x - buildWindow.rect.width/2f,
			y = 0,
			width = buildWindow.rect.width,
			height = buildWindow.position.y
		};
		_placeMode = _hqMode = true;
		_selectedUnit = HQUnit;
		toolTip.HideToolTip();
		InfoBanner.SetText("Place HQ Building");
        HideBuildWindow();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			if (_placeMode && !_hqMode)
				_placeMode = false;
			else
				HideBuildWindow();

		}
		var mPos = Input.mousePosition;
		if (_placeMode && !_buildBarRect.Contains(mPos))
		{
			var t = Map.ActiveMap.GetTileFromRay(_cam.ScreenPointToRay(mPos), _cam.transform.position.y * 2);
			if (t != null && t.Height > Map.ActiveMap.SeaLevel)
			{
				var selectedTiles = Map.ActiveMap.HexSelect(t.Coords, _selectedUnit.tile.size);
				ShowIndicators(ref _selectIndicatorEntities, selectIndicatorEntity, selectedTiles);
				if(_selectedUnit.tile.powerTransferRadius > 0)
				{
					var poweredTiles = Map.ActiveMap.HexSelect(t.Coords, _selectedUnit.tile.powerTransferRadius).Except(selectedTiles).ToList();
					ShowIndicators(ref _powerIndicatorEntities, powerIndicatorEntity, poweredTiles);
				}
				if (Input.GetKeyUp(KeyCode.Mouse0))
				{
					Map.ActiveMap.HexFlatten(t.Coords, 1, 6, Map.FlattenMode.Average);
					Map.ActiveMap.ReplaceTile(t, _selectedUnit.tile);
					if(_hqMode)
					{
						_placeMode = _hqMode = false;
						InfoBanner.SetActive(false);
					}
				}
			}
			else
				HideAllIndicators();
		}
		else
			HideAllIndicators();
	}

	void HideAllIndicators()
	{
		HideIndicators(_selectIndicatorEntities);
		HideIndicators(_powerIndicatorEntities);
	}

	void HideIndicators(NativeArray<Entity> entities)
	{
		for (int i = 0; i < entities.Length; i++)
		{
			if (!_EM.HasComponent<FrozenRenderSceneTag>(entities[i]))
				_EM.AddComponent(entities[i], typeof(FrozenRenderSceneTag));
		}
	}

	void GrowIndicators(ref NativeArray<Entity> entities, MeshEntity meshEntity, int count)
	{
		if (count <= _selectIndicatorEntities.Length)
			return;
		var newEntities = new NativeArray<Entity>(count, Allocator.Persistent);
		for (int i = 0; i < count; i++)
		{
			if (i < _selectIndicatorEntities.Length)
				newEntities[i] = _selectIndicatorEntities[i];
			newEntities[i] = meshEntity.Instantiate(Vector3.zero, Vector3.one * .9f);
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
				if (_EM.HasComponent<FrozenRenderSceneTag>(indicators[i]))
					_EM.RemoveComponent<FrozenRenderSceneTag>(indicators[i]);
				_EM.SetComponentData(indicators[i], new Translation { Value = tiles[i].SurfacePoint });
			}
			else
			{
				if (!_EM.HasComponent<FrozenRenderSceneTag>(indicators[i]))
					_EM.AddComponent(indicators[i], typeof(FrozenRenderSceneTag));
			}
		}
	}

	private void OnDestroy()
	{
		_selectIndicatorEntities.Dispose();
		_powerIndicatorEntities.Dispose();
	}

	public void ShowBuildWindow(UnitInfo[] units)
	{
		HideBuildWindow();
		if (_hqMode)
			return;
		_buildBarRect.height = buildWindow.position.y + buildWindow.rect.height;
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
				_selectedUnit = unit;
				_placeMode = true;
			};
			_activeUnits[i].OnMouseEnter += () => toolTip.ShowToolTip(unit.name, unit.description, unit.GetCostString(), unit.GetProductionString());
			_activeUnits[i].OnMouseExit += () => toolTip.HideToolTip();
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
		buildWindow.gameObject.SetActive(false);
		_buildBarRect.height = buildWindow.position.y;
		for (int i = 0; i < _activeUnits.Length; i++)
		{
			_activeUnits[i]?.gameObject.SetActive(false);
		}
	}
}
