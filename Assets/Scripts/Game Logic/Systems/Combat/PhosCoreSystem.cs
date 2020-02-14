using AnimationSystem.AnimationData;
using Effects.Lines;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PhosCoreSystem : ComponentSystem
{

	private MeshEntityRotatable _bullet;
	private int _state = 0;
	private Map _map;
	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		var op = Addressables.LoadAssetAsync<MeshEntityRotatable>("EnergyPacket");
		op.Completed += e =>
		{
			_bullet = e.Result;
			_state = 1;
		};
		_map = Map.ActiveMap;
	}

	protected override void OnUpdate()
	{
		switch (_state)
		{
			case 0:
				break;
			case 1:
				SimulateAI();
				break;
		}
	}

	private void SimulateAI()
	{
		Entities.WithNone<Disabled>().ForEach((Entity e, ref PhosCore core, ref HexPosition p) =>
		{
			var baseAngle = (((float)Time.ElapsedTime % core.spinRate) / core.spinRate) * (math.PI * 2);
			PostUpdateCommands.SetComponent(core.ring, new Rotation { Value = quaternion.AxisAngle(Vector3.up, baseAngle + (math.PI * 2) / 12f) });
			if (core.nextVolleyTime <= Time.ElapsedTime)
			{
				var unitsInRange = _map.SelectUnitsInRange(p.coords, core.targetingRange);
				if (unitsInRange.Count == 0)
					return;
				var targets = unitsInRange.Take(6).Select(unitId => (float3)_map.units[unitId].Position).ToArray();

				var t = Map.ActiveMap[p.coords];
				FireBurst(t.SurfacePoint, baseAngle, targets, core);
				core.nextVolleyTime = Time.ElapsedTime + core.fireRate;
			}
		});
	}

	private void FireBurst(float3 startPos, float baseAngle, float3[] targets, PhosCore core)
	{
		for (int i = 0; i < 6; i++)
		{
			FirePorjectile(startPos, baseAngle + (math.PI / 3) * i, targets[i % targets.Length], core, Time.ElapsedTime + core.targetDelay + (i * (1 / 12f)));
		}
	}

	private void FireBurst(float3 startPos, float baseAngle, float3 target, PhosCore core)
	{
		for (int i = 0; i < 6; i++)
		{
			FirePorjectile(startPos, baseAngle + (math.PI / 3) * i, target, core, Time.ElapsedTime + core.targetDelay + (i * (1 / 12f)));
		}
	}

	private void FirePorjectile(float3 startPos, float angle, float3 target, PhosCore core, double targetTime)
	{
		var dir = math.rotate(quaternion.RotateY(angle), Vector3.forward);
		var proj = _bullet.BufferedInstantiate(PostUpdateCommands, startPos + (dir * 1.9f) + new float3(0, 2.25916f, 0), Vector3.one * .4f);
		dir.y = .4f;
		PostUpdateCommands.AddComponent(proj, new TimedDeathSystem.DeathTime { Value = Time.ElapsedTime + 5 });
		PostUpdateCommands.AddComponent(proj, new Velocity { Value = dir * core.projectileSpeed });
		PostUpdateCommands.AddComponent(proj, new Drag { Value = 3.2f });
		PostUpdateCommands.AddComponent(proj, new PhosProjectile
		{
			targetTime = targetTime,
			target = target,
			flightSpeed = core.projectileSpeed * 10
		});
	}

	public class PhosProjectileSystem : JobComponentSystem
	{
		struct PhosProjectileJob : IJobForEachWithEntity<PhosProjectile, Velocity, Translation>
		{
			public EntityCommandBuffer.Concurrent CMB;
			public double curTime;

			public void Execute(Entity entity, int index, ref PhosProjectile proj, ref Velocity vel, ref Translation t)
			{
				if (curTime >= proj.targetTime)
				{
					CMB.RemoveComponent(index, entity, typeof(PhosProjectile));
					CMB.RemoveComponent(index, entity, typeof(Gravity));
					CMB.RemoveComponent(index, entity, typeof(Drag));
					vel.Value = math.normalize(proj.target - t.Value) * proj.flightSpeed;
					LineFactory.UpdateStaticLine(CMB, index, entity, t.Value, proj.target);

				}
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var job = new PhosProjectileJob 
			{ 
				CMB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer().ToConcurrent(),
				curTime = Time.ElapsedTime
			};
			inputDeps = job.Schedule(this, inputDeps);
			inputDeps.Complete();
			return inputDeps;
		}
	}


	public struct PhosCore : IComponentData
	{
		public float spinRate;
		public float fireRate;
		public double nextVolleyTime;
		public float projectileSpeed;
		public float targetDelay;
		public int targetingRange;
		public Entity ring;
	}

	public struct PhosProjectile : IComponentData
	{
		public double targetTime;
		public float flightSpeed;
		public float3 target;
	}
}