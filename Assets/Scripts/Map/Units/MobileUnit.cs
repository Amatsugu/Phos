using Amatsugu.Phos.Tiles;
using Amatsugu.Phos.UnitComponents;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine;


namespace Amatsugu.Phos.Units
{
	public class MobileUnit : ICommandable, IMoveable, IAttackState, IAttack, IGroundFire, IGuard, IRepairable, IHaltable
	{
		public int id;
		public MobileUnitEntity info;

		public bool IsRendered { get; protected set; }
		public Faction Faction { get; protected set; }
		public bool IsDead { get; set; }

		protected Map map;


#if DEBUG
		private static bool hasVerified;
#endif

		public MobileUnit(int id, Map map, MobileUnitEntity info, Faction faction)
		{
			this.id = id;
			this.info = info;
			this.map = map;
			//Coords = tile.Coords;
			Faction = faction;
#if DEBUG
			if (!hasVerified)
				ValidateCommands();
#endif
			IsDead = false;
		}

#if DEBUG
		void ValidateCommands()
		{
			ValidateCommad<IAttack>(CommandActions.Attack);
			ValidateCommad<IMoveable>(CommandActions.Move);
			ValidateCommad<IGuard>(CommandActions.Guard);
			ValidateCommad<IAttackState>(CommandActions.AttackState);
			ValidateCommad<IPartolable>(CommandActions.Patrol);
			ValidateCommad<IRepairable>(CommandActions.Repair);
			ValidateCommad<IDeconstructable>(CommandActions.Deconstruct);
			ValidateCommad<IHaltable>(CommandActions.Halt);
			ValidateCommad<IGroundFire>(CommandActions.GroundFire);
			hasVerified = true;
		}

		void ValidateCommad<T>(CommandActions command)
		{
			var cmds = GetSupportedCommands();
			if (cmds.HasFlag(command) && !(this is T))
				Debug.LogError($"<b>{GetType()}</b> reports command support for <b>{command}</b> but does not implement <b>{typeof(T).Name}</b>");
		}

#endif

		public int GetSize()
		{
			return info.size;
		}

		public virtual void Die()
		{
			map.units.Remove(id);
			IsDead = true;
			GameEvents.InvokeOnUnitDied(this);
			Destroy();
			//TODO: Death Effect
		}

		public override int GetHashCode()
		{
			return id;
		}

		public void Destroy()
		{
			
		}

		public void SetState(UnitState.State state)
		{
			throw new NotImplementedException();
		}

		public void Attack(Entity target)
		{
			throw new NotImplementedException();
		}

		public void GoundFire(HexCoords pos)
		{
			throw new NotImplementedException();
		}

		public void Guard(Entity target)
		{
			throw new NotImplementedException();
		}

		public void Repair()
		{
			throw new NotImplementedException();
		}

		public ResourceIndentifier[] GetRepairCost()
		{
			throw new NotImplementedException();
		}

		public void Halt()
		{

		}

		public CommandActions GetSupportedCommands()
		{
			return CommandActions.Attack |
				CommandActions.AttackState |
				CommandActions.Move |
				CommandActions.Guard |
				CommandActions.GroundFire |
				CommandActions.Repair;
		}

		public ScriptableObject GetInfo() => info;

		public float3 GetPosition()
		{
			throw new NotImplementedException();
		}

		public UnitDomain.Domain GetDomain()
		{
			return info.unitDomain;
		}

		public void MoveTo(float3 pos)
		{
			throw new NotImplementedException();
		}
	}
}