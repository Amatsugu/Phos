using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
	public BuildUI buildUI;
	public UIInteractionPanel interactionPanel;
	public Transform selectionBox;

	private Camera _cam;

	private Tile _selectedTile = null;
	private bool _uiBlocked;
	private List<int> _selectedUnits;
	private Tile _start, _end;
	private int groupId;

	void Start()
	{
		_cam = Camera.main;
		_selectedUnits = new List<int>();
		interactionPanel.HidePanel();
		interactionPanel.OnBlur += () => _uiBlocked = false;
		interactionPanel.OnHover += () => _uiBlocked = true;
		interactionPanel.OnUpgradeClick += UpgradeTile;
		interactionPanel.OnDestroyClick += DestroyTile;
		selectionBox.gameObject.SetActive(false);
			//Debug.Log($"R {r} P: {1 + 3*(r + 1)*(r)} C: {s.Count}");
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
						//Drag Select
						var p0 = _start.Coords.worldXZ;
						var p1 = HexCoords.OffsetToWorldPosXZ(_end.Coords.offsetX, _start.Coords.offsetZ, Map.ActiveMap.innerRadius, Map.ActiveMap.tileEdgeLength);
						var p2 = HexCoords.OffsetToWorldPosXZ(_start.Coords.offsetX, _end.Coords.offsetZ, Map.ActiveMap.innerRadius, Map.ActiveMap.tileEdgeLength);
						var p3 = _end.Coords.worldXZ;
#if DEBUG
						p0.y = p1.y = p2.y = p3.y = (_start.Height + _end.Height) / 2f;
						Debug.DrawLine(p0, p1, Color.white);
						Debug.DrawLine(p1, p3, Color.white);
						Debug.DrawLine(p0, p2, Color.white);
						Debug.DrawLine(p2, p3, Color.white);
#endif
						var w = p0.x - p1.x;
						var h = p0.z - p2.z; 
						selectionBox.position = new Vector3(p0.x - (w/2), 0, p0.z - (h/2));
						selectionBox.localScale = new Vector3(w, 100, h);
						selectionBox.gameObject.SetActive(true);
					}
				}
				if (Input.GetKeyUp(KeyCode.Mouse0))
				{
					selectionBox.gameObject.SetActive(false);
					var ray = _cam.ScreenPointToRay(mPos);
					_end = Map.ActiveMap.GetTileFromRay(ray);
					if (_start != null && _end != null)
					{
						_selectedUnits.Clear();
						if (_start == _end)
						{
							ShowPanel(_end);
						}
						else
						{
							/*var selection = Map.ActiveMap.BoxSelect(_start.Coords, _end.Coords);
							var selectionWithUnits = selection.Where(t => t.IsOccupied);
							if (selectionWithUnits.Count() > 0)
							{
								var units = selectionWithUnits.SelectMany(t => t.GetUnits().Where(u => u != 0));
								_selectedUnits.AddRange(units);
							}*/
							_selectedUnits.AddRange(Map.ActiveMap.SelectUnits(_start.Coords, _end.Coords));
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

					var unitGroups = _selectedUnits.GroupBy(u => Map.ActiveMap.units[u].occupiedTile);
					var groupCount = unitGroups.Count();

					var dstTileCount = _selectedUnits.Count / (Tile.MAX_OCCUPANCY-1);
					var r = CalculateR(dstTileCount);
					var dstSelection = Map.ActiveMap.HexSelect(tile.Coords, r).Where(t => !(t is BuildingTile));
					while (dstSelection.Count() < dstTileCount)
						dstSelection = Map.ActiveMap.HexSelect(tile.Coords, ++r).Where(t => !(t is BuildingTile));
					var dstTiles = dstSelection/*.OrderBy(t => Vector3.SqrMagnitude(tile.SurfacePoint - t.SurfacePoint))*/.ToArray();

					var curGroupSize = 0;
					var curGroup = 0;

					foreach (var unitGroup in unitGroups)
					{
						foreach (var unitId in unitGroup)
						{
							Map.ActiveMap.units[unitId].MoveTo(dstTiles[curGroup].SurfacePoint, groupId);

							curGroupSize++;
							if(curGroupSize == Tile.MAX_OCCUPANCY-1)
							{
								curGroupSize = 0;
								curGroup++;
								unchecked
								{
									groupId++;
								}
							}
						}
						unchecked
						{
							groupId++;
						}
					}
				}
			}
		}
		else
			interactionPanel.HidePanel();

		if(interactionPanel.PanelVisible)
		{
			var uiPos = _cam.WorldToScreenPoint(_selectedTile.SurfacePoint);
			if (uiPos.x <= -interactionPanel.Width)
				interactionPanel.HidePanel();
			else if (uiPos.x >= Screen.width)
				interactionPanel.HidePanel();
			if (uiPos.y <= 0)
				interactionPanel.HidePanel();
			else if (uiPos.y >= Screen.height + interactionPanel.Height)
				interactionPanel.HidePanel();

			if (uiPos.x < 0)
				uiPos.x = 0;
			else if (uiPos.x + interactionPanel.Width > Screen.width)
				uiPos.x = Screen.width - interactionPanel.Width;
			if (uiPos.y < interactionPanel.Height)
				uiPos.y = interactionPanel.Height;
			else if (uiPos.y > Screen.height)
				uiPos.y = Screen.height;

			interactionPanel.AnchoredPosition = uiPos;
		}

	}

	int CalculateR(int units)
	{
		var r = 0;
		while ((1 + 3 * (r + 1) * (r)) < units)
			r++;
		return r;
	}

	void ShowPanel(Tile tile)
	{
		_selectedTile = tile;
		switch (tile)
		{
			case HQTile _:
				interactionPanel.ShowPanel(tile.GetName(), tile.GetDescription(), showDestroyBtn: false);
				break;
			case SubHQTile _:
				interactionPanel.ShowPanel(tile.GetName(), tile.GetDescription(), showDestroyBtn: false);
				break;
			case BuildingTile _:
				interactionPanel.ShowPanel(tile.GetName(), tile.GetDescription());
				break;
			case ResourceTile _:
				interactionPanel.ShowPanel(tile.GetName(), tile.GetDescription(), false, false);
				break;
			default:
				interactionPanel.ShowPanel(tile.GetName(), tile.GetDescription(), false, false);
				break;
		}
	}
}
