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

		public Entity Entity;
		public Entity HeadEntity;
		public bool IsRendered { get; protected set; }
		public Faction Faction { get; protected set; }
		public bool IsDead { get; set; }

		private bool _isShown;
		private NativeArray<Entity> _healhBar;
		private Vector3 _startPos;
		protected Map map;
		private NativeArray<Entity> _subMeshes;


#if DEBUG
		private static bool hasVerified;
#endif

		public MobileUnit(int id, Map map, MobileUnitEntity info, Tile tile, Faction faction)
		{
			this.id = id;
			this.info = info;
			this.map = map;
			_startPos = tile.SurfacePoint;
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

		public Entity Render()
		{
			if (IsRendered)
				return Entity;
			IsRendered = _isShown = true;
			Entity = info.Instantiate(_startPos, Quaternion.identity, id, Faction);
			RenderSubMeshes();
			GameRegistry.EntityManager.SetComponentData(Entity, new FactionId { Value = Faction });
			if (info.healthBar != null)
				_healhBar = info.healthBar.Instantiate(Entity, info.centerOfMassOffset + info.healthBarOffset);
			return Entity;
		}

		public virtual void RenderSubMeshes()
		{
			if (info.subMeshes.Length == 0)
				return;
			_subMeshes = info.InstantiateSubMeshes(quaternion.identity, Entity);

			if (info.head.id != -1)
			{
				HeadEntity = _subMeshes[info.head.id];
				GameRegistry.EntityManager.AddComponentData(Entity, new UnitHead { Value = HeadEntity });
			}
		}

		public void Show(bool isShown)
		{
			if (isShown == _isShown)
				return;
			if (_isShown = isShown)
			{
				GameRegistry.EntityManager.RemoveComponent(Entity, typeof(DisableRendering));
				GameRegistry.EntityManager.RemoveComponent(HeadEntity, typeof(DisableRendering));
				if (_healhBar.IsCreated)
					GameRegistry.EntityManager.RemoveComponent(_healhBar, typeof(DisableRendering));
			}
			else
			{
				GameRegistry.EntityManager.AddComponent(Entity, typeof(DisableRendering));
				GameRegistry.EntityManager.AddComponent(HeadEntity, typeof(DisableRendering));
				if (_healhBar.IsCreated)
					GameRegistry.EntityManager.AddComponent(_healhBar, typeof(DisableRendering));
			}
		}

		public void MoveTo(float3 pos)
		{
			if (GameRegistry.EntityManager.HasComponent<MoveToTarget>(Entity))
				GameRegistry.EntityManager.RemoveComponent<MoveToTarget>(Entity);
			if (!GameRegistry.EntityManager.HasComponent<Destination>(Entity))
			{
				GameRegistry.EntityManager.AddComponent(Entity, typeof(Destination));
			}
			if (GameRegistry.EntityManager.HasComponent<Path>(Entity))
			{
				GameRegistry.EntityManager.RemoveComponent<PathProgress>(Entity);
				GameRegistry.EntityManager.RemoveComponent<Path>(Entity);
			}
			GameRegistry.EntityManager.SetComponentData(Entity, new Destination { Value = pos });
		}

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
			try
			{
				GameRegistry.EntityManager.DestroyEntity(Entity);
				if (_healhBar.IsCreated)
					GameRegistry.EntityManager.DestroyEntity(_healhBar);
				GameRegistry.EntityManager.DestroyEntity(_subMeshes);
				IsRendered = false;
			}
			catch (Exception e)
			{
				Debug.LogWarning(e);
			}finally
			{
				if (_healhBar.IsCreated)
					_healhBar.Dispose();
				if (_subMeshes.IsCreated)
					_subMeshes.Dispose();
			}
		}

		public void SetState(UnitState.State state)
		{
			if (GameRegistry.EntityManager.HasComponent<UnitState>(Entity))
				GameRegistry.EntityManager.SetComponentData(Entity, new UnitState { Value = state });
			else
				GameRegistry.EntityManager.AddComponentData(Entity, new UnitState { Value = state });
		}

		public void Attack(Entity target)
		{
			if (!GameRegistry.EntityManager.HasComponent<MoveToTarget>(Entity))
				GameRegistry.EntityManager.AddComponent<MoveToTarget>(Entity);
			if (GameRegistry.EntityManager.HasComponent<AttackTarget>(Entity))
				GameRegistry.EntityManager.SetComponentData(Entity, new AttackTarget { Value = target });
			else
				GameRegistry.EntityManager.AddComponentData(Entity, new AttackTarget { Value = target });
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
			var cost = GetRepairCost();
			ResourceSystem.ConsumeResourses(cost);
			GameRegistry.EntityManager.SetComponentData(Entity, new Health
			{
				Value = info.maxHealth
			});

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
			return GameRegistry.EntityManager.GetComponentData<CenterOfMass>(Entity).Value;
		}

		public UnitDomain.Domain GetDomain()
		{
			return info.unitDomain;
		}
	}
}