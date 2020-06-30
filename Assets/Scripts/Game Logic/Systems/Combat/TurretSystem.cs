using Amatsugu.Phos.Tiles;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
			var op = Addressables.LoadAssetAsync<ProjectileMeshEntity>("PlayerProjectile");
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
				var r = EntityManager.GetComponentData<Rotation>(t.Head).Value;
				var tgtPos = EntityManager.GetComponentData<CenterOfMass>(attackTarget.Value).Value;
				tgtPos.y = pos.Value.y;
				var desR = quaternion.LookRotation(pos.Value - tgtPos, math.up());
				desR = Quaternion.RotateTowards(r, desR, 360 * Time.DeltaTime);
				EntityManager.SetComponentData(t.Head, new Rotation
				{
					Value = desR
				});
			});

			//Idle
			Entities.WithAll<Turret>().ForEach((Entity e, ref Translation pos, ref AttackRange range, ref FactionId faction) =>
			{
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

			//Shoot
			Entities.ForEach((Entity e, ref Turret t, ref Translation pos, ref AttackSpeed speed, ref AttackRange range, ref AttackTarget attackTarget) =>
			{
				var r = EntityManager.GetComponentData<Rotation>(t.Head).Value;
				var tgtPos = EntityManager.GetComponentData<CenterOfMass>(attackTarget.Value).Value;
				var dir = math.normalizesafe(pos.Value - tgtPos);
				var flatDir = dir;
				flatDir.y = 0;
				var desR = quaternion.LookRotation(flatDir, math.up());
				if (r.value.Equals(desR.value))
					return;
				if (Time.ElapsedTime < speed.NextAttackTime)
					return;
				speed.NextAttackTime += speed.Value;
				var b = _bullet.Instantiate(pos.Value + math.up() * 2, 0.2f, -dir);
				PostUpdateCommands.AddComponent(b, new DeathTime { Value = Time.ElapsedTime + 5 });
			});
		}
	}

	public struct Turret : IComponentData
	{
		internal Entity Head;
	}
}
