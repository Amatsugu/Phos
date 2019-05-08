using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
	public BuildUI buildUI;
	public RectTransform selectionRect;
	public UIInteractionPanel interactionPanel;

	private Camera _cam;

	private Tile _selectedTile = null;
	private bool _uiBlocked;
	private List<int> _selectedUnits;
	private Tile _start, _end;


	void Start()
	{
		_cam = Camera.main;
		_selectedUnits = new List<int>();
		interactionPanel.HidePanel();
		interactionPanel.OnBlur += () => _uiBlocked = false;
		interactionPanel.OnHover += () => _uiBlocked = true;
		interactionPanel.OnUpgradeClick += UpgradeTile;
		interactionPanel.OnDestroyClick += DestroyTile;
		selectionRect.gameObject.SetActive(false);
		for (int r = 0; r < 10; r++)
		{
			var s = Map.ActiveMap.HexSelect(HexCoords.FromPosition(new Vector3(100, 0, 100), 1), r);
			Debug.Log($"R {r} P: {1 + 3*(r + 1)*(r)} C: {s.Count}");
		}
	}

	void DestroyTile()
	{
		var t = _selectedTile as BuildingTile;
		Map.ActiveMap.RevertTile(t);
		ResourceSystem.AddResources(t.buildingInfo.cost, .5f);
		interactionPanel.HidePanel();
	}

	void UpgradeTile()
	{

	}

	void LateUpdate()
	{
		if (buildUI.hqMode)
			return;
		var mPos = Input.mousePosition;
		if (Input.GetKeyUp(KeyCode.Escape))
			interactionPanel.HidePanel();
		if (!buildUI.placeMode)
		{
			if (!_uiBlocked && !buildUI.uiBlock)
			{

				if (Input.GetKeyDown(KeyCode.Mouse0))
				{
					var ray = _cam.ScreenPointToRay(mPos);
					_start = Map.ActiveMap.GetTileFromRay(ray);
					interactionPanel.HidePanel();
				}
				if(Input.GetKey(KeyCode.Mouse0))
				{
					var ray = _cam.ScreenPointToRay(mPos);
					_end = Map.ActiveMap.GetTileFromRay(ray);
					if(_start != _end && _start != null && _end != null)
					{
						selectionRect.gameObject.SetActive(true);
						//Drag Select
						var p0 = _start.Coords.worldXZ;
						var p1 = HexCoords.OffsetToWorldPosXZ(_end.Coords.offsetX, _start.Coords.offsetZ, Map.ActiveMap.innerRadius, Map.ActiveMap.tileEdgeLength);
						var p2 = HexCoords.OffsetToWorldPosXZ(_start.Coords.offsetX, _end.Coords.offsetZ, Map.ActiveMap.innerRadius, Map.ActiveMap.tileEdgeLength);
						var p3 = _end.Coords.worldXZ;
						p0.y = p1.y = p2.y = p3.y = (_start.Height + _end.Height) / 2f;
						Debug.DrawLine(p0, p1, Color.white);
						Debug.DrawLine(p1, p3, Color.white);
						Debug.DrawLine(p0, p2, Color.white);
						Debug.DrawLine(p2, p3, Color.white);
						var sp0 = _cam.WorldToScreenPoint(p0);
						var sp3 = _cam.WorldToScreenPoint(p3);
						Vector3 left = sp0;
						if (sp0.x > sp3.x)
							left.x = sp3.x;
						if (sp0.y < sp3.y)
							left.y = sp3.y;
						selectionRect.position = left;
						selectionRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Abs(sp3.x - sp0.x));
						selectionRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Abs(sp3.y - sp0.y));
					}
				}
				if (Input.GetKeyUp(KeyCode.Mouse0))
				{
					selectionRect.gameObject.SetActive(false);
					var ray = _cam.ScreenPointToRay(mPos);
					_end = Map.ActiveMap.GetTileFromRay(ray);
					if (_start != null && _end != null)
					{
						_selectedUnits.Clear();
						if (_start == _end)
						{
							if (_end.IsOccupied)
							{
								_selectedUnits.AddRange(_end.GetUnits().Where(u => u != 0));
							}
							else if (_end is BuildingTile)
							{
								ShowPanel(_end);
							}
						}else
						{
							var selection = Map.ActiveMap.BoxSelect(_start.Coords, _end.Coords);
							var selectionWithUnits = selection.Where(t => t.IsOccupied);
							if (selectionWithUnits.Count() > 0)
							{
								var units = selectionWithUnits.SelectMany(t => t.GetUnits().Where(u => u != 0));
								_selectedUnits.AddRange(units);
							}

						}
					}
				}
			}
			if(Input.GetKeyUp(KeyCode.Mouse1) && _selectedUnits.Count > 0)
			{
				var ray = _cam.ScreenPointToRay(mPos);
				var tile = Map.ActiveMap.GetTileFromRay(ray);
				if(tile != null)
				{
					IEnumerable<Tile> selection = Map.ActiveMap.HexSelect(tile.Coords, 1).Where(t => !(t is BuildingTile));
					var r = 1;
					while(selection.Count() < _selectedUnits.Count)
					{
						selection = Map.ActiveMap.HexSelect(tile.Coords, ++r).Where(t => !(t is BuildingTile));
					}
					var s = selection.ToArray();
					for (int i = 0; i < _selectedUnits.Count; i++)
					{
						Map.ActiveMap.units[_selectedUnits[i]].MoveTo(s[i].SurfacePoint);
					}
				}
			}
		}
		else
			interactionPanel.HidePanel();

		if(interactionPanel.PanelVisible)
		{
			var uiPos = _cam.WorldToScreenPoint(_selectedTile.SurfacePoint);
			if (uiPos.x < 0)
				uiPos.x = 0;
			if (uiPos.x + interactionPanel.Width > Screen.width)
				uiPos.x = Screen.width - interactionPanel.Width;
			if (uiPos.y < 0)
				uiPos.y = interactionPanel.Height;
			if (uiPos.y > Screen.height)
				uiPos.y = Screen.height;
			interactionPanel.AnchoredPosition = uiPos;
		}

	}

	void ShowPanel(Tile tile)
	{
		_selectedTile = tile;
		switch (tile)
		{
			case HQTile _:
				interactionPanel.ShowPanel(tile.info.name, tile.info.description, showDestroyBtn: false);
				break;
			case SubHQTile _:
				interactionPanel.ShowPanel(tile.info.name, tile.info.description, showDestroyBtn: false);
				break;
			case PoweredBuildingTile p:
				interactionPanel.ShowPanel(tile.info.name, $"{tile.info.description}\n\n<b>HQ Connection: {p.HasHQConnection}</b>");
				break;
			case BuildingTile _:
				interactionPanel.ShowPanel(tile.info.name, tile.info.description);
				break;
			default:
				interactionPanel.ShowPanel(tile.info.name, tile.info.description, false, false);
				break;
		}
	}
}
