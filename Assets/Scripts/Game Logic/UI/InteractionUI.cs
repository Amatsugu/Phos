using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
	public BuildUI buildUI;
	public UIInteractionPanel interactionPanel;

	private Camera _cam;

	private Tile _selectedTile = null;
	private bool _uiBlocked;
	private int _id = -1;
	private Tile _start, _end;


	void Start()
	{
		_cam = Camera.main;
		interactionPanel.HidePanel();
		interactionPanel.OnBlur += () => _uiBlocked = false;
		interactionPanel.OnHover += () => _uiBlocked = true;
		interactionPanel.OnUpgradeClick += UpgradeTile;
		interactionPanel.OnDestroyClick += DestroyTile;
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
						Debug.DrawLine(p0, p1, Color.white);
						Debug.DrawLine(p1, p3, Color.white);
						Debug.DrawLine(p0, p2, Color.white);
						Debug.DrawLine(p2, p3, Color.white);
					}
				}
				if (Input.GetKeyUp(KeyCode.Mouse0))
				{
					var ray = _cam.ScreenPointToRay(mPos);
					_end = Map.ActiveMap.GetTileFromRay(ray);
					if (_start == _end)
					{
						if (_end != null)
						{
							if (_end.IsOccupied)
							{
								Debug.Log($"Unit Selected {_id = _end.GetUnits()[0]}");
							}
							else if (_end is BuildingTile)
							{
								ShowPanel(_end);
							}
						}
					}
				}
			}
			if(Input.GetKeyUp(KeyCode.Mouse1) && _id != -1)
			{
				var ray = _cam.ScreenPointToRay(mPos);
				var tile = Map.ActiveMap.GetTileFromRay(ray);
				if(tile != null)
				{
					Map.ActiveMap.units[_id].MoveTo(tile.SurfacePoint);
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
