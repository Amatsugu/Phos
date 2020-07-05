using Amatsugu.Phos.Tiles;

using Effects.Lines;

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIActionsPanel : UIPanel
{
	public UIBuildPanel actionButtonPrefab;

	[Header("Indicators")]
	public MeshEntityRotatable moveLine;

	[Header("Buttons")]
	public Button attackCommand;
	public Button moveCommand;
	public Button attackStateCommand;
	public Button guardCommand;
	public Button repairCommand;
	public Button deconstructCommand;
	public Button groundFireCommand;
	public Button patrolCommand;
	public Button haltCommand;

	private HashSet<Button> _activeButtons;
	private ActionState _actionState;
	private CommandActions _selectedCommand;
	private Vector3 _mousePos;
	private Tile _selectedTile;
	private Camera _cam;
	private BuildPhysicsWorld _physicsWorld;
	private List<ICommandable> _selectedEntities;
	private Map _map;
	private float3 _lastOrderTarget;

	public enum ActionState
	{
		Disabled,
		IssueCommand
	}
	
	protected override void Awake()
	{
		base.Awake();
		_activeButtons = new HashSet<Button>();
		HideAllButtons();

		_physicsWorld = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
		_selectedEntities = new List<ICommandable>();
		SetupCallbacks();
		GameEvents.OnMapLoaded += OnMapLoad;
		GameEvents.OnUnitDied += OnUnitDied;
	}

	private void OnUnitDied(ICommandable unit)
	{
		_selectedEntities.Remove(unit);
	}

	private void OnMapLoad()
	{
		_map = GameRegistry.GameMap;
	}

	void SetupCallbacks()
	{
		attackCommand.onClick.AddListener(() => SetState(CommandActions.Attack));
		moveCommand.onClick.AddListener(() => SetState(CommandActions.Move));
		attackStateCommand.onClick.AddListener(() => SetState(CommandActions.AttackState));
		groundFireCommand.onClick.AddListener(() => SetState(CommandActions.GroundFire));
		guardCommand.onClick.AddListener(() => SetState(CommandActions.Guard));
		repairCommand.onClick.AddListener(() => SetState(CommandActions.Repair));
		deconstructCommand.onClick.AddListener(() => SetState(CommandActions.Deconstruct));
		patrolCommand.onClick.AddListener(() => SetState(CommandActions.Patrol));
		haltCommand.onClick.AddListener(() => SetState(CommandActions.Halt));
	}

	protected override void Start()
	{
		base.Start();
		_cam = GameRegistry.Camera;
	}

	private void SetState(CommandActions command)
	{
		if (!IsOpen)
			return;
		_actionState = ActionState.IssueCommand;
		_selectedCommand = command;
		Debug.Log($"State: {command}");
	}

	public void UpdateState()
	{
		if (_actionState == ActionState.Disabled)
			return;
		_mousePos = Input.mousePosition;
		
		switch (_selectedCommand)
		{
			case CommandActions.Attack:
				UpdateAttack();
				break;
			case CommandActions.AttackState:
				UpdateAttackState();
				break;
			case CommandActions.Move:
				UpdateMove();
				break;
			case CommandActions.GroundFire:
				UpdateGroundFire();
				break;
			case CommandActions.Guard:
				UpdateGuard();
				break;
			case CommandActions.Repair:
				UpdateRepair();
				break;
			case CommandActions.Deconstruct:
				UpdateDeconstruct();
				break;
			case CommandActions.Patrol:
				UpdatePartol();
				break;
			case CommandActions.Halt:
				UpdateHalt();
				break;
		}
	}

	private void GetTile()
	{
		var ray = _cam.ScreenPointToRay(_mousePos);
		var hasTile = _physicsWorld.GetTileFromRay(ray, _cam.transform.position.y * 2, out var pos);
		if (hasTile)
			_selectedTile = _map[pos];
		else
			return;
	}

	private void UpdateAttack()
	{
		if (!Input.GetKeyUp(KeyCode.Mouse1))
			return;
		var ray = _cam.ScreenPointToRay(_mousePos);
		var hasHit = _physicsWorld.PhysicsWorld.CastRay(new RaycastInput
		{
			Start = ray.origin,
			End = ray.GetPoint(_cam.transform.position.y * 2),
			Filter = new CollisionFilter
			{
				CollidesWith = 1u << (int)Faction.Phos,
				BelongsTo = ~0u
			}
		}, out var hit);
		Debug.Log(hasHit);
		if(hasHit)
		{
			IssueAttackOrder(hit.Entity);
		}
	}

	private void IssueAttackOrder(Entity target)
	{
		for (int i = 0; i < _selectedEntities.Count; i++)
		{
			((IAttack)_selectedEntities[i]).Attack(target);
		}
	}

	private void UpdateAttackState()
	{

	}

	private void UpdateMove()
	{
		if(!_lastOrderTarget.Equals(float3.zero))
			RenderMove();

		if (!Input.GetKeyUp(KeyCode.Mouse1))
			return;
		GetTile();
		if (_selectedTile.info.isTraverseable)
			IssueMoveOrder(_selectedTile);


	}

	private void RenderMove()
	{
		var center = float3.zero;
		var deathTime = Time.time + 0.01f;
		for (int i = 0; i < _selectedEntities.Count; i++)
		{
			var pos = _selectedEntities[i].GetPosition();
			center += pos;
			var ln = LineFactory.CreateStaticLine(moveLine, pos, pos + math.up(), 0.05f);
			Map.EM.AddComponentData(ln, new DeathTime { Value = deathTime });
		}
		center /= _selectedEntities.Count;
		center += math.up();
		for (int i = 0; i < _selectedEntities.Count; i++)
		{
			var pos = _selectedEntities[i].GetPosition();
			var ln = LineFactory.CreateStaticLine(moveLine, pos + math.up(), center, 0.05f);
			Map.EM.AddComponentData(ln, new DeathTime { Value = deathTime });
		}
		var ln2 = LineFactory.CreateStaticLine(moveLine, center, _lastOrderTarget + math.up(), 0.05f);
		Map.EM.AddComponentData(ln2, new DeathTime { Value = deathTime });
	}

	private void IssueMoveOrder(Tile tile)
	{
		if (tile.IsUnderwater)
			return;
		_lastOrderTarget = tile.SurfacePoint;
		var tilesNeeded = 0;
		for (int i = 0; i < _selectedEntities.Count; i++)
			tilesNeeded += HexCoords.GetTileCount((_selectedEntities[i] as IMoveable).GetSize());

		var r = HexCoords.CalculateRadius(tilesNeeded) + 1;
		var orderedUnits = _selectedEntities.OrderBy(u => ((IMoveable)u).GetSize()).Reverse().Select(u => u as IMoveable).ToArray();

		var occupiedSet = new HashSet<HexCoords>();
		var openSet = new HashSet<HexCoords>();

		var openTiles = HexCoords.SpiralSelect(tile.Coords, r);
		for (int i = 0; i < openTiles.Length; i++)
			openSet.Add(openTiles[i]);
		for (int i = 0; i < orderedUnits.Length; i++)
		{
			for (int j = 0; j < openTiles.Length; j++)
			{
				var footprint = HexCoords.SpiralSelect(openTiles[j], orderedUnits[i].GetSize());
				if (IsValidFootPrint(footprint, openSet, occupiedSet))
				{
					for (int x = 0; x < footprint.Length; x++)
						occupiedSet.Add(footprint[x]);
					orderedUnits[i].MoveTo(_map[openTiles[j]].SurfacePoint);
					/*var order = new MoveOrder
					{
						unit = orderedUnits[i],
						dst = _map[openTiles[j]].SurfacePoint
					};
					order.cost = (order.dst - order.unit.Position).sqrMagnitude;
					UnityEngine.Debug.DrawRay(order.dst, Vector3.up, Color.magenta, 1);
					_moveOrderQueue.Enqueue(order);*/

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
			if (_map[coord].IsUnderwater)
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

	private void UpdateGroundFire()
	{

	}

	private void UpdateGuard()
	{

	}

	private void UpdateRepair()
	{

	}

	private void UpdateDeconstruct()
	{

	}

	private void UpdatePartol()
	{

	}

	private void UpdateHalt()
	{

	}

	public override void Show()
	{
		base.Show();
		SetState(CommandActions.Move);
	}

	public void ResetSelection()
	{
		_selectedEntities.Clear();
	}

	public void AddSelectedEntity(ICommandable entity)
	{
		_selectedEntities.Add(entity);
		var supportedCommands = entity.GetSupportedCommands();
		if (supportedCommands.HasFlag(CommandActions.Attack))
			Showbutton(attackCommand);

		if (supportedCommands.HasFlag(CommandActions.Move))
			Showbutton(moveCommand);

		if (supportedCommands.HasFlag(CommandActions.AttackState))
			Showbutton(attackStateCommand);

		if (supportedCommands.HasFlag(CommandActions.Guard))
			Showbutton(guardCommand);

		if (supportedCommands.HasFlag(CommandActions.Repair))
			Showbutton(repairCommand);

		if (supportedCommands.HasFlag(CommandActions.Deconstruct))
			Showbutton(deconstructCommand);

		if (supportedCommands.HasFlag(CommandActions.GroundFire))
			Showbutton(groundFireCommand);

		if (supportedCommands.HasFlag(CommandActions.Patrol))
			Showbutton(patrolCommand);

		if (supportedCommands.HasFlag(CommandActions.Halt))
			Showbutton(haltCommand);
	}

	private void Showbutton(Button button)
	{
		if (!_activeButtons.Contains(button))
		{
			button.gameObject.SetActive(true);
			_activeButtons.Add(button);
		}
	}

	public override void Hide()
	{
		base.Hide();
		HideAllButtons();
		_activeButtons.Clear();
		_selectedEntities.Clear();
		_actionState = ActionState.Disabled;
	}

	private void HideAllButtons()
	{
		attackCommand.gameObject.SetActive(false);
		moveCommand.gameObject.SetActive(false);
		attackStateCommand.gameObject.SetActive(false);
		groundFireCommand.gameObject.SetActive(false);
		guardCommand.gameObject.SetActive(false);
		repairCommand.gameObject.SetActive(false);
		deconstructCommand.gameObject.SetActive(false);
		patrolCommand.gameObject.SetActive(false);
		haltCommand.gameObject.SetActive(false);
	}
}