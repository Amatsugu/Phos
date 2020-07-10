using Amatsugu.Phos.Tiles;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Amatsugu.Phos.ECS
{
	public class TurretSystem : ComponentSystem
	{

		private bool _isReady = false;
		private ProjectileMeshEntity _bullet;
		private BuildPhysicsWorld _physicsWorld;
		private StepPhysicsWorld _simWorld;
		private NativeList<int> _castHits;
		private CollisionFilter _playerTargetingFilter;
		private CollisionFilter _phosTargetingFilter;

		protected override void OnCreate()
		{
			base.OnCreate();
			var op = Addressables.LoadAssetAsync<ProjectileMeshEntity>("PlayerLaser");
			op.Completed += e =>
			{
				if (e.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
				{
					_bullet = e.Result;
					_isReady = true;
				}
			};
			_physicsWorld = World.GetExistingSystem<BuildPhysicsWorld>();
			_simWorld = World.GetExistingSystem<StepPhysicsWorld>();
			_castHits = new NativeList<int>(Allocator.Persistent);
			_playerTargetingFilter = new CollisionFilter
			{
				BelongsTo = (1u << (int)Faction.Player),
				CollidesWith = (1u << (int)Faction.Phos)
			};
			_phosTargetingFilter = new CollisionFilter
			{
				BelongsTo = (1u << (int)Faction.Phos),
				CollidesWith = (1u << (int)Faction.Player)
			};
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			_castHits.Dispose();
		}

		protected override void OnUpdate()
		{
			if (!_isReady)
				return;

			var world = _physicsWorld.PhysicsWorld;
			_castHits.Clear();

			//Verify Target
			Entities.WithAll<Turret>().ForEach((Entity e, ref AttackTarget target) =>
			{
				
			});

			//Aim
			Entities.ForEach((Entity e, ref Turret t, ref Translation pos, ref AttackSpeed speed, ref AttackRange range, ref AttackTarget attackTarget) =>
			{
				if (!EntityManager.Exists(attackTarget.Value))
				{
					PostUpdateCommands.RemoveComponent<AttackTarget>(e);
					return;
				}
				var r = EntityManager.GetComponentData<Rotation>(t.Head).Value;
				var tgtPos = EntityManager.GetComponentData<CenterOfMass>(attackTarget.Value).Value;
				var dir = pos.Value - tgtPos;
				dir.y = 0;
				var desR = Quaternion.LookRotation(dir, math.up());
				desR = Quaternion.RotateTowards(r, desR, 360 * Time.DeltaTime);
				EntityManager.SetComponentData(t.Head, new Rotation
				{
					Value = desR
				});
			});

			//Idle/Select Target
			Entities.WithNone<AttackTarget>().WithAll<Turret>().ForEach((Entity e, ref Translation pos, ref AttackRange range, ref FactionId faction, ref AttackSpeed speed) =>
			{
				if (Time.ElapsedTime < speed.NextAttackTime)
					return;
				speed.NextAttackTime = Time.ElapsedTime + speed.Value;
				_physicsWorld.AABBCast(pos.Value, range.Value, faction.Value == Faction.Player ? _playerTargetingFilter : _phosTargetingFilter, ref _castHits);
				for (int i = 0; i < _castHits.Length; i++)
				{
					var tgtE = _physicsWorld.PhysicsWorld.Bodies[_castHits[i]].Entity;
					var tgtPos = EntityManager.GetComponentData<CenterOfMass>(tgtE).Value;
					var distSq = math.lengthsq(pos.Value - tgtPos);
					if(distSq < range.ValueSq)
					{
						PostUpdateCommands.AddComponent(e, new AttackTarget { Value = tgtE });
						break;
					}
				}
			});

			Entities.WithNone<AttackTarget>().ForEach((ref Turret t) =>
			{
				var r = EntityManager.GetComponentData<Rotation>(t.Head).Value;
				r = math.mul(math.normalizesafe(r), quaternion.AxisAngle(math.up(), math.radians(10) * Time.DeltaTime));
				PostUpdateCommands.SetComponent(t.Head, new Rotation { Value = r });
			});

			//Shoot
			Entities.ForEach((Entity e, ref Turret t, ref Translation pos, ref AttackSpeed speed, ref AttackRange range, ref AttackTarget attackTarget) =>
			{
				if (Time.ElapsedTime < speed.NextAttackTime)
					return;
				speed.NextAttackTime = Time.ElapsedTime + speed.Value;
				if (!EntityManager.Exists(attackTarget.Value))
				{
					PostUpdateCommands.RemoveComponent<AttackTarget>(e);
					return;
				}
				var r = EntityManager.GetComponentData<Rotation>(t.Head).Value;
				var tgtPos = EntityManager.GetComponentData<CenterOfMass>(attackTarget.Value).Value;
				var dir = math.normalizesafe(pos.Value - tgtPos);
				var flatDir = dir;
				flatDir.y = 0;
				var desR = quaternion.LookRotation(flatDir, math.up());
				if (r.value.Equals(desR.value))
					return;

				var shotPos = EntityManager.GetComponentData<Translation>(t.Head).Value + math.rotate(r, t.shotOffset);
				DebugUtilz.DrawCrosshair(shotPos, 0.1f, Color.magenta, 0.1f);
				var b = _bullet.Instantiate(shotPos, .2f, -dir * 10f);
				if(_bullet.nonUniformScale)
					PostUpdateCommands.SetComponent(b, new NonUniformScale { Value = new float3(0.2f, 0.2f, .6f) });
				PostUpdateCommands.AddComponent(b, new DeathTime { Value = Time.ElapsedTime + 5 });
			});
		}
	}

	public struct Turret : IComponentData
	{
		internal Entity Head;
		public float3 shotOffset;
	}
}
