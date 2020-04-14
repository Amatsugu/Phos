using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

using UnityEngine;

public class GameUI : UIHover
{
	public Transform selectionBox;
	private UIInfoPanel _infoPanel;
	private UIBuildPanel _buildPanel;
	private UIActionsPanel _actionsPanel;
	private UICategoryPanel _categoryPanel;

	private UIState state;

	private BuildPhysicsWorld _buildPhysicsWorld;
	private NativeList<int> _castHits;
	private Tile _start;
	private Tile _end;

	private enum UIState
	{
		Disabled,
		Idle,
		PlaceBuilding,
		PlaceUnit,
		BuildingsSelected,
		UnitsSelected,
	}

	protected override void Awake()
	{
		base.Awake();
		_castHits = new NativeList<int>(Allocator.Persistent);
		selectionBox.gameObject.SetActive(false);
		_infoPanel = GetComponentInChildren<UIInfoPanel>(true);
		_infoPanel.enabled = true;
		_buildPanel = GetComponentInChildren<UIBuildPanel>(true);
		_buildPanel.enabled = true;
		_actionsPanel = GetComponentInChildren<UIActionsPanel>(true);
		_actionsPanel.enabled = true;
		_categoryPanel = GetComponentInChildren<UICategoryPanel>(true);
		_categoryPanel.enabled = true;

		_categoryPanel.OnButtonClicked += CategorySelected;
		_buildPanel.OnHide += OnBuildPanelClosed;

		_buildPanel.infoPanel = _infoPanel;

		GameEvents.OnGameReady += Init;
	}

	private void Init()
	{
		state = UIState.PlaceBuilding;
		_buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
	}

	private void CategorySelected(BuildingCategory category)
	{
		state = UIState.PlaceBuilding;
		_buildPanel.Show(category);
	}

	private void OnBuildPanelClosed()
	{
		state = UIState.Idle;
	}

	protected override void Update()
	{
		base.Update();
		if (!isHovered)
			_buildPanel.UpdateState();
		if (state == UIState.Idle)
		{
			SelectionUI();
		}
	}

	private void SelectionUI()
	{
		var mousePos = Input.mousePosition;
		var cam = GameRegistry.Camera;
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			var ray = cam.ScreenPointToRay(mousePos);
			_start = Map.ActiveMap.GetTileFromRay(ray);
		}
		if (Input.GetKey(KeyCode.Mouse0))
		{
			var ray = cam.ScreenPointToRay(mousePos);
			_end = Map.ActiveMap.GetTileFromRay(ray);
			if (_start != _end && _start != null && _end != null)
				DisplaySelectionRect();
		}
		if (Input.GetKeyUp(KeyCode.Mouse0))
		{
			selectionBox.gameObject.SetActive(false);
			var ray = cam.ScreenPointToRay(mousePos);
			_end = Map.ActiveMap.GetTileFromRay(ray);
			if (_start != null && _end != null)
			{
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
				if (_castHits.Length > 0)
				{
					var selection = new List<MobileUnit>();
					for (int i = 0; i < _castHits.Length; i++)
					{
						var entity = _buildPhysicsWorld.PhysicsWorld.Bodies[_castHits[i]].Entity;
						if(Map.EM.HasComponent<UnitId>(entity))
							selection.Add(Map.ActiveMap.units[Map.EM.GetComponentData<UnitId>(entity).Value]);
					}
					_actionsPanel.ShowApplicableButtons(selection.ToArray());
				}
				//_selectedUnits.AddRange(Map.ActiveMap.SelectUnits(_start.Coords, _end.Coords));
			}
		}
	}

	private void DisplaySelectionRect()
	{
		//Drag Select
		var p0 = _start.Coords.world;
		var p1 = HexCoords.OffsetToWorldPosXZ(_end.Coords.offsetCoords.x, _start.Coords.offsetCoords.y, Map.ActiveMap.innerRadius, Map.ActiveMap.tileEdgeLength);
		var p2 = HexCoords.OffsetToWorldPosXZ(_start.Coords.offsetCoords.x, _end.Coords.offsetCoords.y, Map.ActiveMap.innerRadius, Map.ActiveMap.tileEdgeLength);
		var p3 = _end.Coords.world;
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
}