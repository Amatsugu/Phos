using System.Linq;

using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.AddressableAssets;

public class PhosCoreSystem : ComponentSystem
{
	private DynamicMeshEntity _bullet;
	private int _state = 0;
	private Map _map;

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		var op = Addressables.LoadAssetAsync<DynamicMeshEntity>("EnemyProjectile");
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
		Entities.WithNone<Disabled>().ForEach((Entity e, ref PhosCore core, ref HexPosition p, ref FactionId team) =>
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
				FireBurst(t.SurfacePoint, baseAngle, targets, core, team);
				core.nextVolleyTime = Time.ElapsedTime + core.fireRate;
			}
		});
	}

	private void FireBurst(float3 startPos, float baseAngle, float3[] targets, PhosCore core, FactionId team)
	{
		for (int i = 0; i < 6; i++)
		{
			FirePorjectile(startPos, baseAngle + (math.PI / 3) * i, targets[i % targets.Length], core, Time.ElapsedTime + core.targetDelay + (i * (1 / 12f)), team);
		}
	}

	private void FireBurst(float3 startPos, float baseAngle, float3 target, PhosCore core, FactionId team)
	{
		for (int i = 0; i < 6; i++)
		{
			FirePorjectile(startPos, baseAngle + (math.PI / 3) * i, target, core, Time.ElapsedTime + core.targetDelay + (i * (1 / 12f)), team);
		}
	}

	private void FirePorjectile(float3 startPos, float angle, float3 target, PhosCore core, double targetTime, FactionId team)
	{
		var dir = math.rotate(quaternion.RotateY(angle), Vector3.forward);
		var pos = startPos + (dir * 2.9f) + new float3(0, 4, 0);
		dir.y = .4f;
		var vel = dir * core.projectileSpeed;
		var proj = _bullet.BufferedInstantiate(PostUpdateCommands, pos, quaternion.identity, vel);
		PostUpdateCommands.SetComponent(proj, new Scale { Value = .5f });
		PostUpdateCommands.AddComponent(proj, new TimedDeathSystem.DeathTime { Value = Time.ElapsedTime + 15 });
		PostUpdateCommands.AddComponent(proj, new PhysicsGravityFactor { Value = 0 });
		PostUpdateCommands.AddComponent(proj, team);
		PostUpdateCommands.AddComponent(proj, new Damage
		{
			Value = 25
		});
		PostUpdateCommands.AddComponent(proj, new PhosProjectile
		{
			targetTime = targetTime,
			target = target,
			flightSpeed = core.projectileSpeed * 15
		});
	}
}

[BurstCompile]
public class PhosProjectileSystem : JobComponentSystem
{
	private struct PhosProjectileJob : IJobForEachWithEntity<PhosProjectile, PhysicsVelocity, Translation>
	{
		public EntityCommandBuffer.Concurrent CMB;
		public double curTime;

		public void Execute(Entity entity, int index, ref PhosProjectile proj, ref PhysicsVelocity vel, ref Translation t)
		{
			if (curTime >= proj.targetTime)
			{
				CMB.RemoveComponent(index, entity, typeof(PhosProjectile));
				vel.Linear = math.normalize(proj.target - t.Value) * proj.flightSpeed;
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