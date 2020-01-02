using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class InteractionUI : MonoBehaviour
{
	public UIInteractionPanel interactionPanel;
	public Transform selectionBox;

	public float maxMoveCostPerFrame = 100;

	private Camera _cam;

	private Tile _selectedTile = null;
	private bool _uiBlocked;
	private List<int> _selectedUnits;
	private Tile _start, _end;
	private Queue<MoveOrder> _moveOrderQueue;

	public struct MoveOrder
	{
		public Vector3 dst;
		public MobileUnit unit;
		public float cost;

		public void Complete()
		{
			unit.MoveTo(dst);
		}
	}

	void Awake()
	{
		GameRegistry.INST.interactionUI = this;
	}

	void Start()
	{
		_cam = GameRegistry.Camera;
		_moveOrderQueue = new Queue<MoveOrder>();
		_selectedUnits = new List<int>();
		interactionPanel.HidePanel();
		interactionPanel.OnBlur += () => _uiBlocked = false;
		interactionPanel.OnHover += () => _uiBlocked = true;
		interactionPanel.OnUpgradeClick += UpgradeTile;
		interactionPanel.OnDestroyClick += DestroyTile;
		selectionBox.gameObject.SetActive(false);
		EventManager.AddEventListener("nameWindowOpen", () =>
		{
			interactionPanel.HidePanel();
			enabled = false;
		});
		EventManager.AddEventListener("nameWindowClose", () =>
		{
			enabled = true;
		});
		
	}

	private void OnValidate()
	{
		_moveOrderQueue = new Queue<MoveOrder>();
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

	void Update()
	{
		if (GameRegistry.BuildUI.hqMode)
			return;
		var mPos = Input.mousePosition;
		if (Input.GetKeyUp(KeyCode.Escape))
			interactionPanel.HidePanel();

		if (!GameRegistry.BuildUI.placeMode)
		{
			if (!_uiBlocked && !GameRegistry.BuildUI.uiBlock && !NotificationsUI.INST.isHovered)
			{
				if (Input.GetKeyDown(KeyCode.Mouse0))
				{
					var ray = _cam.ScreenPointToRay(mPos);
					_start = Map.ActiveMap.GetTileFromRay(ray);
					HidePanel();
				}
				if (Input.GetKey(KeyCode.Mouse0))
				{
					var ray = _cam.ScreenPointToRay(mPos);
					_end = Map.ActiveMap.GetTileFromRay(ray);
					if (_start != _end && _start != null && _end != null)
						DisplaySelectionRect();
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
							ShowPanel(_end);
						else
						{
							_selectedUnits.AddRange(Map.ActiveMap.SelectUnits(_start.Coords, _end.Coords));
						}
					}
				}
			}
			if (Input.GetKeyUp(KeyCode.Mouse1) && _selectedUnits.Count > 0)
			{
				var ray = _cam.ScreenPointToRay(mPos);
				var tile = Map.ActiveMap.GetTileFromRay(ray);
				if (tile != null)
				{
					InstructUnitMovement(tile);
				}
			}
		}
		else
			HidePanel();

	}

	void LateUpdate()
	{
#if DEBUG
		for (int i = 0; i < _selectedUnits.Count; i++)
		{
			var unit = Map.ActiveMap.units[_selectedUnits[i]];
			Debug.DrawRay(unit.Position, Vector3.up, Color.white);
		}
#endif

		if(interactionPanel.PanelVisible)
		{
			var uiPos = _cam.WorldToScreenPoint(_selectedTile.SurfacePoint);
			if (uiPos.x <= -interactionPanel.Width)
				HidePanel();
			else if (uiPos.x >= Screen.width)
				HidePanel();
			if (uiPos.y <= 0)
				HidePanel();
			else if (uiPos.y >= Screen.height + interactionPanel.Height)
				HidePanel();

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
		var curCost = 0f;
		while(curCost < maxMoveCostPerFrame && _moveOrderQueue.Count > 0)
		{
			var order = _moveOrderQueue.Dequeue();
			curCost += order.cost;
			order.Complete();
		}

	}

	void DisplaySelectionRect()
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
		selectionBox.position = new Vector3(p0.x - (w / 2), 0, p0.z - (h / 2));
		selectionBox.localScale = new Vector3(w, 80, h);
		selectionBox.gameObject.SetActive(true);
	}

	void InstructUnitMovement(Tile tile)
	{
		var tilesNeeded = 0;
		for (int i = 0; i < _selectedUnits.Count; i++)
			tilesNeeded += HexCoords.GetTileCount(Map.ActiveMap.units[_selectedUnits[i]].info.size);
		var r = HexCoords.CalculateRadius(tilesNeeded)+1;
		var orderedUnits = _selectedUnits.Select(uId => Map.ActiveMap.units[uId]).OrderBy(u => u.info.size).Reverse().ToArray();

		var occupiedSet = new HashSet<HexCoords>();
		var openSet = new HashSet<HexCoords>();

		var openTiles = HexCoords.HexSelect(tile.Coords, r);
		for (int i = 0; i < openTiles.Length; i++)
			openSet.Add(openTiles[i]);
		for (int i = 0; i < orderedUnits.Length; i++)
		{
			for (int j = 0; j < openTiles.Length; j++)
			{
				var footprint = HexCoords.HexSelect(openTiles[j], orderedUnits[i].info.size);
				if(IsValidFootPrint(footprint, openSet, occupiedSet))
				{
					for (int x = 0; x < footprint.Length; x++)
						occupiedSet.Add(footprint[x]);
					var order = new MoveOrder
					{
						unit = orderedUnits[i],
						dst = Map.ActiveMap[openTiles[j]].SurfacePoint
					};
					order.cost = (order.dst - order.unit.Position).sqrMagnitude;
					_moveOrderQueue.Enqueue(order);

					break;
				}
			}
		}
	}

	bool IsValidFootPrint(HexCoords[] footprint, HashSet<HexCoords> open, HashSet<HexCoords> occupied)
	{
		bool isValid = true;
		for (int i = 0; i < footprint.Length; i++)
		{
			var coord = footprint[i];
			if(Map.ActiveMap[coord].IsUnderwater)
			{
				isValid = false;
				break;
			}
			if (!open.Contains(coord))
			{
				isValid = false;
				break;
			}
			if(occupied.Contains(coord))
			{
				isValid = false;
				break;
			}
		}

		return isValid;
	}

	void HidePanel()
	{
		interactionPanel.HidePanel();
	}

	void ShowPanel(Tile tile)
	{
		_selectedTile = tile;
		interactionPanel.rTransform.position = tile.SurfacePoint;
		
		switch (tile)
		{
			case HQTile _:
				//GameRegistry.ResearchTreeUI.Show(null);
				interactionPanel.ShowPanel(tile.GetName(), tile.GetDescription(), showDestroyBtn: false);
				break;
			case SubHQTile _:
				//GameRegistry.ResearchTreeUI.Show(null);
				interactionPanel.ShowPanel(tile.GetName(), tile.GetDescription(), showDestroyBtn: false);
				break;
			case ResearchBuildingTile rb:
				if(rb.HasHQConnection)
					GameRegistry.ResearchTreeUI.Show(rb);
				//interactionPanel.ShowPanel(tile.GetName(), tile.GetDescription());
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
