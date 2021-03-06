﻿#if false
using Amatsugu.Phos.ECS.Jobs.Pathfinder;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.UI;
using static Amatsugu.Phos.ECS.Jobs.Pathfinder.PathFinder;
using Debug = UnityEngine.Debug;

public class InteractionUI : MonoBehaviour
{
	public UIInteractionPanel interactionPanel;
	public Transform selectionBox;

	public float maxMoveCostPerFrame = 100;
	public GraphicRaycaster raycaster;
	public EventSystem eventSystem;

	private Camera _cam;

	private Tile _selectedTile = null;
	private bool _uiBlocked;
	private List<int> _selectedUnits;
	private Tile _start, _end;
	private Queue<MoveOrder> _moveOrderQueue;

	private InteractionState _curState;
	private NativeHashMap<HexCoords, float> _navData;
	private BuildPhysicsWorld _buildPhysicsWorld;
	private NativeList<int> _castHits;
	private Map _map;

	private enum InteractionState
	{
		Diabled,
		Inspect,
		OrderUnit
	}

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

	private void Awake()
	{
		GameRegistry.INST.interactionUI = this;
		GameEvents.OnMapRegen += OnRegen;
	}

	void OnRegen()
	{
		interactionPanel.HidePanel();
		_map = GameRegistry.GameMap;
	}

	private void Start()
	{
		_buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
		_castHits = new NativeList<int>(Allocator.Persistent);
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
			_curState = InteractionState.Diabled; interactionPanel.HidePanel();
		});
		EventManager.AddEventListener("nameWindowClose", () =>
		{
			_curState = InteractionState.Inspect;
		});

		GameRegistry.BuildUI.OnBuildWindowOpen +=  () =>
		{
			interactionPanel.HidePanel();
			_curState = InteractionState.Diabled;
		};
		GameRegistry.BuildUI.OnBuildWindowClose += () =>
		{
			UnityEngine.Debug.Log("Build Closed");
			_curState = InteractionState.Inspect;
		};
		_navData = _map.GenerateNavData();
	}

	private void OnValidate()
	{
		_moveOrderQueue = new Queue<MoveOrder>();
	}

	private void DestroyTile()
	{
		var t = _selectedTile as BuildingTile;
		_map.RevertTile(t);
		ResourceSystem.AddResources(t.buildingInfo.cost, .5f);
		interactionPanel.HidePanel();
	}

	private void UpgradeTile()
	{
	}

	private void Update()
	{
		var mPos = Input.mousePosition;
		
		switch (_curState)
		{
			case InteractionState.Diabled:
				if (GameRegistry.BuildUI.State == BuildUI.BuildState.Disabled)
					_curState = InteractionState.Inspect;
				break;

			case InteractionState.Inspect:
				ProcessCloseInput();
				InspectUI(mPos);
				break;

			case InteractionState.OrderUnit:
				ProcessCloseInput();
				InspectUI(mPos);
				InstructUnitUI(mPos);
				break;
		}
	}

	private void ProcessCloseInput()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			interactionPanel.HidePanel();
			_curState = InteractionState.Inspect;
		}
	}

	private void InstructUnitUI(Vector2 mousePos)
	{
		if (Input.GetKeyUp(KeyCode.Mouse1) && _selectedUnits.Count > 0)
		{
			var ray = _cam.ScreenPointToRay(mousePos);
			var tile = _map.GetTileFromRay(ray);
			if (tile != null)
			{
				InstructUnitMovement(tile);
			}
		}
	}

	private void InspectUI(Vector2 mousePos)
	{
		if (!_uiBlocked && !NotificationsUI.INST.isHovered)
		{
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				var ray = _cam.ScreenPointToRay(mousePos);
				_start = _map.GetTileFromRay(ray);
				HidePanel();
			}
			if (Input.GetKey(KeyCode.Mouse0))
			{
				var ray = _cam.ScreenPointToRay(mousePos);
				_end = _map.GetTileFromRay(ray);
				if (_start != _end && _start != null && _end != null)
					DisplaySelectionRect();
			}
			if (Input.GetKeyUp(KeyCode.Mouse0))
			{
				selectionBox.gameObject.SetActive(false);
				var ray = _cam.ScreenPointToRay(mousePos);
				_end = _map.GetTileFromRay(ray);
				if (_start != null && _end != null)
				{
					_selectedUnits.Clear();
					if (_start == _end)
						ShowPanel(_end);
					else
					{
						_curState = InteractionState.OrderUnit;
						var bounds = MathUtils.PhysicsBounds(_start.SurfacePoint, _end.SurfacePoint);
						bounds.Max.y = 50;
						bounds.Min.y = 0;
						DebugUtilz.DrawCrosshair(bounds.Min, 50, Color.cyan, 0);
						DebugUtilz.DrawCrosshair(bounds.Max, 50, Color.cyan, 0);
						_buildPhysicsWorld.AABBCast(bounds, new CollisionFilter
						{
							BelongsTo = 1u << (int)Faction.Player,
							CollidesWith = 1u << (int)Faction.Unit
						}, ref _castHits);
						if(_castHits.Length > 0)
						{
							_selectedUnits.Clear();
							for (int i = 0; i < _castHits.Length; i++)
							{
								var entity = _buildPhysicsWorld.PhysicsWorld.Bodies[_castHits[i]].Entity;
								_selectedUnits.Add(Map.EM.GetComponentData<UnitId>(entity).Value);
							}
						}
						//_selectedUnits.AddRange(_map.SelectUnits(_start.Coords, _end.Coords));

					}
				}
			}
		}
	}

	private void LateUpdate()
	{
#if DEBUG
		for (int i = 0; i < _selectedUnits.Count; i++)
		{
			if (!_map.units.ContainsKey(_selectedUnits[i]))
				continue;
			var unit = _map.units[_selectedUnits[i]];
			var pos = Map.EM.GetComponentData<Translation>(unit.Entity).Value;
			Debug.DrawRay(pos, Vector3.up, Color.white);
		}
#endif

		if (interactionPanel.PanelVisible)
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
		while (curCost < maxMoveCostPerFrame && _moveOrderQueue.Count > 0)
		{
			var order = _moveOrderQueue.Dequeue();
			curCost += order.cost;
			order.Complete();
		}
	}

	private void DisplaySelectionRect()
	{
		//Drag Select
		var p0 = _start.Coords.world;
		var p1 = HexCoords.OffsetToWorldPosXZ(_end.Coords.offsetCoords.x, _start.Coords.offsetCoords.y, _map.innerRadius, _map.tileEdgeLength);
		var p2 = HexCoords.OffsetToWorldPosXZ(_start.Coords.offsetCoords.x, _end.Coords.offsetCoords.y, _map.innerRadius, _map.tileEdgeLength);
		var p3 = _end.Coords.world;
#if DEBUG
		p0.y = p1.y = p2.y = p3.y = (_start.Height + _end.Height) / 2f;
		UnityEngine.Debug.DrawLine(p0, p1, Color.white);
		UnityEngine.Debug.DrawLine(p1, p3, Color.white);
		UnityEngine.Debug.DrawLine(p0, p2, Color.white);
		UnityEngine.Debug.DrawLine(p2, p3, Color.white);
#endif
		var w = p0.x - p1.x;
		var h = p0.z - p2.z;
		selectionBox.position = new Vector3(p0.x - (w / 2), 0, p0.z - (h / 2));
		selectionBox.localScale = new Vector3(w, 80, h);
		selectionBox.gameObject.SetActive(true);
	}

	private void InstructUnitMovement(Tile tile)
	{
		var tilesNeeded = 0;
		for (int i = 0; i < _selectedUnits.Count; i++)
		{
			if(_map.units.ContainsKey(_selectedUnits[i]))
				tilesNeeded += HexCoords.GetTileCount(_map.units[_selectedUnits[i]].info.size);
		}
		var r = HexCoords.CalculateRadius(tilesNeeded) + 1;
		var orderedUnits = _selectedUnits.Select(uId => _map.units[uId]).OrderBy(u => u.info.size).Reverse().ToArray();

		var occupiedSet = new HashSet<HexCoords>();
		var openSet = new HashSet<HexCoords>();

		var openTiles = HexCoords.SpiralSelect(tile.Coords, r);
		for (int i = 0; i < openTiles.Length; i++)
			openSet.Add(openTiles[i]);
		for (int i = 0; i < orderedUnits.Length; i++)
		{
			for (int j = 0; j < openTiles.Length; j++)
			{
				var footprint = HexCoords.SpiralSelect(openTiles[j], orderedUnits[i].info.size);
				if (IsValidFootPrint(footprint, openSet, occupiedSet))
				{
					/*for (int x = 0; x < footprint.Length; x++)
						occupiedSet.Add(footprint[x]);
					var order = new MoveOrder
					{
						unit = orderedUnits[i],
						dst = _map[openTiles[j]].SurfacePoint
					};
					order.cost = (order.dst - order.unit.Position).sqrMagnitude;
					UnityEngine.Debug.DrawRay(order.dst, Vector3.up, Color.magenta, 1);
					_moveOrderQueue.Enqueue(order);
					*/
					break;
				}
			}
		}
	}

	private bool IsValidFootPrint(HexCoords[] footprint, HashSet<HexCoords> open, HashSet<HexCoords> occupied)
	{
		bool isValid = true;
		for (int i = 0; i < footprint.Length; i++)
		{
			var coord = footprint[i];
			if (GameRegistry.GameMap[coord].IsUnderwater)
			{
				isValid = false;
				break;
			}
			if (!open.Contains(coord))
			{
				isValid = false;
				break;
			}
			if (occupied.Contains(coord))
			{
				isValid = false;
				break;
			}
		}

		return isValid;
	}

	private void HidePanel()
	{
		interactionPanel.HidePanel();
	}

	private void ShowPanel(Tile tile)
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
				if (rb.HasHQConnection)
					GameRegistry.ResearchTreeUI.Show(rb);
				//interactionPanel.ShowPanel(tile.GetName(), tile.GetDescription());
				break;
			case BuildingTile b when b.buildingInfo.faction != Faction.Phos:
				interactionPanel.ShowPanel(tile.GetName(), tile.GetDescription(), showDestroyBtn: false, showUpgradeBtn: false);
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

	private void OnDestroy()
	{
		_castHits.Dispose();
		GameEvents.OnMapRegen -= OnRegen;
	}
}
#endif