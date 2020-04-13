using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

public enum CommandActions
{
	Move = 0,
	Attack = 1,
	Deconstruct = 2,
	Guard = 4,
	GroundFire = 8,
	Repair = 16,
	Patrol = 32,
	AttackState = 64, 
	Halt = 128
}

public interface ICommandable
{
	CommandActions GetSupportedCommands();
}

public interface IDeconstructable
{
	void Deconstruct();
}

public interface IRepairable
{
	void Repair();
	ResourceIndentifier[] GetRepairCost();
}

public interface IGuard
{
	void Guard(Entity target);
}

public interface IAttack
{
	void Attack(Entity target);
}

public interface IGroundFire
{
	void GoundFire(HexCoords pos);
}

public interface IAttackState
{
	void SetState(UnitState.State state);
}

public interface IMoveable
{
	void MoveTo(float3 pos);
}

public interface IPartolable
{
	void AddWayPoint(HexCoords pos);

	void ClearPatrol();
}

public interface IHaltable
{
	void Halt();
}