using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Physics.Systems;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIActionsPanel : UIPanel
{
	public UIBuildPanel actionButtonPrefab;

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

	public enum ActionState
	{
		Disabled,
		IssueCommand
	}

	private struct MoveOrder
	{
		public Vector3 dst;
		public MobileUnit unit;
		public float cost;

		public void Complete()
		{
			unit.MoveTo(dst);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_activeButtons = new HashSet<Button>();
		HideAllButtons();

		_physicsWorld = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
		_selectedEntities = new List<ICommandable>();
		SetupCallbacks();
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
			_selectedTile = Map.ActiveMap[pos];
		else
			return;
	}

	private void UpdateAttack()
	{

	}

	private void UpdateAttackState()
	{

	}

	private void UpdateMove()
	{
		if (!Input.GetKeyUp(KeyCode.Mouse1))
			return;
		GetTile();
		if (_selectedTile.info.isTraverseable)
			IssueMoveOrder(_selectedTile);
	}

	private void IssueMoveOrder(Tile tile)
	{
		var tilesNeeded = 0;
		for (int i = 0; i < _selectedEntities.Count; i++)
		{
			tilesNeeded += HexCoords.GetTileCount((_selectedEntities[i] as IMoveable).GetSize());
		}
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
				var footprint = HexCoords.SpiralSelect(openTiles[j], ((IMoveable)orderedUnits[i]).GetSize());
				if (IsValidFootPrint(footprint, openSet, occupiedSet))
				{
					for (int x = 0; x < footprint.Length; x++)
						occupiedSet.Add(footprint[x]);
					orderedUnits[i].MoveTo(Map.ActiveMap[openTiles[j]].SurfacePoint);
					/*var order = new MoveOrder
					{
						unit = orderedUnits[i],
						dst = Map.ActiveMap[openTiles[j]].SurfacePoint
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
			if (Map.ActiveMap[coord].IsUnderwater)
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