using System.Linq;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.AddressableAssets;

[BurstCompile]
[UpdateAfter(typeof(BuildPhysicsWorld))]
public class PhosCoreSystem : ComponentSystem
{
	private int _state = 0;
	private Map _map;
	private ProjectileMeshEntity projectile;
	private ProjectileMeshEntity laser;
	private BuildPhysicsWorld buildPhysics;
	private NativeList<int> _inRangeList;
	private NativeArray<float3> _curTargets;

	protected override void OnCreate()
	{
		base.OnCreate();
		GameEvents.OnMapLoaded += Init;
		buildPhysics = World.GetOrCreateSystem<BuildPhysicsWorld>();
	}

	protected void Init()
	{
		_map = Map.ActiveMap;
		var projLoad = Addressables.LoadAssetAsync<ProjectileMeshEntity>("EnemyProjectile");
		var laserLoad = Addressables.LoadAssetAsync<ProjectileMeshEntity>("EnemyLaser");
		_inRangeList = new NativeList<int>(Allocator.Persistent);
		_curTargets = new NativeArray<float3>(6, Allocator.Persistent);
		projLoad.Completed += r =>
		{
			if (r.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
			{
				projectile = r.Result;
				_state = 1;
			}
		};
		laserLoad.Completed += r =>
		{
			if (r.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
				laser = r.Result;
		};
		GameEvents.OnMapLoaded -= Init;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_inRangeList.Dispose();
		_curTargets.Dispose();
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
		Entities.WithNone<Disabled>().ForEach((Entity e, ref PhosCore core, ref PhosCoreData data, ref Translation t, ref FactionId faction) =>
		{
			var baseAngle = (((float)Time.ElapsedTime % core.spinRate) / core.spinRate) * (math.PI * 2); //Angle of the ring
			PostUpdateCommands.SetComponent(data.ring, new Rotation { Value = quaternion.AxisAngle(Vector3.up, baseAngle + (math.PI * 2) / 12f) });
			if (core.nextVolleyTime <= Time.ElapsedTime)
			{
				_inRangeList.Clear();
				buildPhysics.AABBCast(t.Value, new float3(core.targetingRange), new CollisionFilter
				{
					BelongsTo = 1u << (int)faction.Value,
					CollidesWith = ~((1u << (int)faction.Value) | (1u << (int)Faction.None) | (1u << (int)Faction.PlayerProjectile) | (1u << (int)Faction.PhosProjectile) | (1u << (int)Faction.Tile) | (1u << (int)Faction.Unit)),
					GroupIndex = 0
				}, ref _inRangeList);
				if (_inRangeList.Length == 0)
					return;
				for (int i = 0; i < 6; i++)
				{
					var targetEntity = buildPhysics.PhysicsWorld.Bodies[_inRangeList[i % _inRangeList.Length]].Entity;
					var target = EntityManager.GetComponentData<CenterOfMass>(targetEntity).Value;
					if (math.lengthsq(target - t.Value) <= core.targetingRangeSq)
						_curTargets[i] = target;
				}
				FireBurst(t.Value, baseAngle, _curTargets, core, faction);
				core.nextVolleyTime = Time.ElapsedTime + core.fireRate;
			}
		});
	}

	private void FireBurst(float3 startPos, float baseAngle, NativeArray<float3> targets, PhosCore core, FactionId team)
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
		var proj = projectile.BufferedInstantiate(PostUpdateCommands, pos, .5f, vel);
		PostUpdateCommands.AddComponent(proj, new DeathTime { Value = Time.ElapsedTime + 15 });

		PostUpdateCommands.AddComponent(proj, new PhosProjectile
		{
			targetTime = targetTime,
			target = target,
			flightSpeed = core.projectileSpeed * 15
		});
	}
}

[BurstCompile]
[UpdateAfter(typeof(BuildPhysicsWorld))]
public class PhosProjectileSystem : JobComponentSystem
{
	[BurstCompile]
	private struct PhosProjectileJob : IJobChunk //IJobForEachWithEntity<PhosProjectile, PhysicsVelocity, Translation>
	{
		public double curTime;
		[ReadOnly] public ArchetypeChunkComponentType<PhosProjectile> projectileType;
		public ArchetypeChunkComponentType<PhysicsVelocity> velocityType;
		[ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var vel = chunk.GetNativeArray(velocityType);
			var proj = chunk.GetNativeArray(projectileType);
			var trans = chunk.GetNativeArray(translationType);
			for (int i = 0; i < chunk.Count; i++)
			{
				if (curTime < proj[i].targetTime)
					continue;
				var v = vel[i];
				v.Linear = math.normalize(proj[i].target - trans[i].Value) * proj[i].flightSpeed;
				vel[i] = v;
			}
		}
	}

	private EntityQuery entityQuery;

	protected override void OnCreate()
	{
		base.OnCreate();
		var desc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				ComponentType.ReadOnly<PhosProjectile>(),
				typeof(PhysicsVelocity),
				ComponentType.ReadOnly<Translation>(),
			},
			None = new ComponentType[]
			{
				typeof(Disabled)
			}
		};
		entityQuery = GetEntityQuery(desc);
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var job = new PhosProjectileJob
		{
			curTime = Time.ElapsedTime,
			projectileType = GetArchetypeChunkComponentType<PhosProjectile>(true),
			velocityType = GetArchetypeChunkComponentType<PhysicsVelocity>(false),
			translationType = GetArchetypeChunkComponentType<Translation>(true),
		};
		////inputDeps = job.Schedule(this, inputDeps);
		inputDeps = job.ScheduleParallel(entityQuery, inputDeps);
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
	public int targetingRangeSq;
}

public struct PhosCoreData : IComponentData
{
	public Entity ring;
	public Entity projectile;
	public Entity laser;
}

public struct PhosProjectile : IComponentData
{
	public double targetTime;
	public float flightSpeed;
	public float3 target;
}