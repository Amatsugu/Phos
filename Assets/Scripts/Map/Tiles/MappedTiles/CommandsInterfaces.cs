using Amatsugu.Phos;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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
	bool IsDead { get; set; }

	CommandActions GetSupportedCommands();

	float3 GetPosition();

	ScriptableObject GetInfo();
}

public interface IDeconstructable
{
	void Deconstruct(DynamicBuffer<GenericPrefab> prefabs, DynamicBuffer<TileInstance> tiles, EntityCommandBuffer postUpdateCommands);

	bool CanDeconstruct(Faction faction);

	ResourceIndentifier[] GetResourceRefund();
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

	int GetSize();

	UnitDomain.Domain GetDomain();

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