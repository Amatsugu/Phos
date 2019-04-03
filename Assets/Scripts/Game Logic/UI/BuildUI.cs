using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;

public class BuildUI : MonoBehaviour
{
	public UnitInfo[] Tech;
	public UnitInfo[] Resource;
	public UnitInfo[] Economy;
	public UnitInfo[] Structure;
	public UnitInfo[] Millitary;
	public UnitInfo[] Defense;

	//UI
	public Transform selector;
	public MeshEntity selectorEntity;
	public RectTransform buildWindow;
	public RectTransform toolTip;
	public TMP_Text toolTipTitle;
	public TMP_Text toolTipBody;
	public TMP_Text toolTipCost;
	public TMP_Text toolTipProd;
	public Vector2 tooltipOffset;

	public RectTransform unitUIPrefab;

	private RectTransform[] _activeUnits;
	private UnitInfo _selectedUnit;
	private bool _placeMode;
	private Camera _cam;
	private bool _toolTipVisible;
	private NativeArray<Entity> _selectionEntities;
	private EntityManager _EM;
	private Rect _buildBarRect;

	void Start()
	{
		buildWindow.gameObject.SetActive(false);
		_activeUnits = new RectTransform[6];
		ShowBuildWindow(Tech);
		ShowToolTip(Tech[0].name, Tech[0].description, Tech[0].GetCostString(), Tech[0].GetProductionString());
		_cam = Camera.main;
		_selectionEntities = new NativeArray<Entity>(0, Allocator.Persistent);
		_EM = World.Active.EntityManager;//Map.EM;
		_buildBarRect = new Rect
		{
			x = buildWindow.position.x - buildWindow.rect.width/2f,
			y = 0,
			width = buildWindow.rect.width,
			height = buildWindow.position.y + buildWindow.rect.height
		};
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
			_placeMode = false;
		var mPos = Input.mousePosition;
		if (_placeMode && !_buildBarRect.Contains(mPos))
		{
			var t = Map.ActiveMap.GetTileFromRay(_cam.ScreenPointToRay(mPos), _cam.transform.position.y * 2);
			if (t != null && t.Height > Map.ActiveMap.SeaLevel)
			{
				var selectedTiles = Map.ActiveMap.HexSelect(t.Coords, _selectedUnit.tile.size);
				if (_selectionEntities.Length < selectedTiles.Count)
				{
					GrowIndicators(ref _selectionEntities, selectedTiles.Count);
				}
				for (int i = 0; i < _selectionEntities.Length; i++)
				{
					if (i < selectedTiles.Count)
					{
						if(_EM.HasComponent<FrozenRenderSceneTag>(_selectionEntities[i]))
							_EM.RemoveComponent<FrozenRenderSceneTag>(_selectionEntities[i]);
						_EM.SetComponentData(_selectionEntities[i], new Translation { Value = selectedTiles[i].SurfacePoint });
					}
					else
					{
						if(!_EM.HasComponent<FrozenRenderSceneTag>(_selectionEntities[i]))
							_EM.AddComponent(_selectionEntities[i], typeof(FrozenRenderSceneTag));
					}
				}
				if (Input.GetKeyUp(KeyCode.Mouse0))
				{
					Map.ActiveMap.HexFlatten(t.Coords, 1, 6, Map.FlattenMode.Average);
					Map.ActiveMap.ReplaceTile(t, _selectedUnit.tile);
				}
			}
			else
				HideIndicators(_selectionEntities);
		}
		else
			HideIndicators(_selectionEntities);

		if (_toolTipVisible)
		{
			var pos = Input.mousePosition + (Vector3)tooltipOffset;
			pos.x = Mathf.Clamp(pos.x, 0, Screen.width - toolTip.rect.width);
			pos.y = Mathf.Clamp(pos.y, 0, Screen.height - toolTip.rect.height);
			toolTip.anchoredPosition = pos;
		}
	}

	void HideIndicators(NativeArray<Entity> entities)
	{
		for (int i = 0; i < entities.Length; i++)
		{
			if (!_EM.HasComponent<FrozenRenderSceneTag>(entities[i]))
				_EM.AddComponent(entities[i], typeof(FrozenRenderSceneTag));
		}
	}

	void GrowIndicators(ref NativeArray<Entity> entities, int count)
	{
		if (count <= _selectionEntities.Length)
			return;
		var newEntities = new NativeArray<Entity>(count, Allocator.Persistent);
		for (int i = 0; i < count; i++)
		{
			if (i < _selectionEntities.Length)
				newEntities[i] = _selectionEntities[i];
			newEntities[i] = selectorEntity.Instantiate(Vector3.zero, Vector3.one * .9f);
		}
		entities.Dispose();
		entities = newEntities;
	}

	private void OnDestroy()
	{
		_selectionEntities.Dispose();
	}

	public void ShowToolTip(string title, string body, string costInfo, string productionInfo)
	{
		toolTip.gameObject.SetActive(_toolTipVisible = true);
		toolTipTitle.SetText(title);
		toolTipBody.SetText(body);
		toolTipCost.SetText(costInfo);
		toolTipProd.SetText(productionInfo);
	}

	public void HideToolTip()
	{
		toolTip.gameObject.SetActive(_toolTipVisible = false);
	}

	public void ShowBuildWindow(UnitInfo[] units)
	{
		buildWindow.gameObject.SetActive(true);
		for (int i = 0; i < units.Length; i++)
		{
			var unit = units[i];
			if(_activeUnits[i] == null)
			{
				_activeUnits[i] = Instantiate(unitUIPrefab, buildWindow);
				_activeUnits[i].anchoredPosition = new Vector2(5 + (i * 170), 5);
			}
			_activeUnits[i].GetComponentInChildren<TMP_Text>().SetText(unit.name);
			var btn = _activeUnits[i].GetComponent<Button>();
			btn.onClick.RemoveAllListeners();
			btn.onClick.AddListener(() =>
			{
				ShowToolTip(unit.name, unit.description, unit.GetCostString(), unit.GetProductionString());
				_selectedUnit = unit;
				_placeMode = true;
			});
		}
	}

	public void HideBuildWindow()
	{
		buildWindow.gameObject.SetActive(false);
		for (int i = 0; i < _activeUnits.Length; i++)
		{
			_activeUnits[i]?.gameObject.SetActive(false);
		}
	}
}
