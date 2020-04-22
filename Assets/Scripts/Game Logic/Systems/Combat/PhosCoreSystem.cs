using System.Linq;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Profiling;

[BurstCompile]
[UpdateBefore(typeof(BuildPhysicsWorld))]
[UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
public class PhosCoreSystem : JobComponentSystem
{
	private int _state = 0;
	private Map _map;
	private ProjectileMeshEntity projectile;
	private ProjectileMeshEntity laser;
	private Entity projectileEntity;
	private BuildPhysicsWorld buildPhysics;
	private NativeList<int> _inRangeList;
	private NativeArray<float3> _curTargets;
	private EntityQuery _entityQuery;
	private EndSimulationEntityCommandBufferSystem _endSimulation;


	private struct PhosTargetingJob : IJobChunk
	{
		public ArchetypeChunkComponentType<PhosCore> coreType;
		[ReadOnly] public ArchetypeChunkComponentType<PhosCoreData> dataType;
		[ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
		[ReadOnly] public ArchetypeChunkComponentType<FactionId> factionType;
		[ReadOnly] public Entity projectile;
		[ReadOnly] public ComponentDataFromEntity<CenterOfMass> centerOfMass;
		public CollisionWorld colWorld;
		public PhysicsWorld physWorld;
		public EntityCommandBuffer.Concurrent cmb;
		public float elapsed;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var cores = chunk.GetNativeArray(coreType);
			var data = chunk.GetNativeArray(dataType);
			var translations = chunk.GetNativeArray(translationType);
			var factions = chunk.GetNativeArray(factionType);

			Profiler.BeginSample("Phos System");
			var inRangeList = new NativeList<int>(Allocator.Temp);
			var curTargets = new NativeArray<float3>(6, Allocator.Temp);
			for (int i = 0; i < chunk.Count; i++)
			{
				var core = cores[i];
				inRangeList.Clear();
				var baseAngle = ((elapsed % core.spinRate) / core.spinRate) * (math.PI * 2); //Angle of the ring
				cmb.SetComponent(chunkIndex, data[i].ring, new Rotation { Value = quaternion.AxisAngle(math.up(), baseAngle + (math.PI * 2) / 12f) });
				if (core.nextVolleyTime <= elapsed)
				{
					var faction = factions[i];
					var t = translations[i];
					Profiler.BeginSample("Phos System AABBCast");
					colWorld.OverlapAabb(new OverlapAabbInput
					{
						Aabb = new Aabb { Max = t.Value + core.targetingRange / 2f, Min = t.Value - core.targetingRange / 2f },
						Filter = new CollisionFilter
						{
							BelongsTo = 1u << (int)faction.Value,
							CollidesWith = ~((1u << (int)faction.Value) | (1u << (int)Faction.None) | (1u << (int)Faction.PlayerProjectile) | (1u << (int)Faction.PhosProjectile) | (1u << (int)Faction.Tile) | (1u << (int)Faction.Unit)),
							GroupIndex = 0
						}
					}, ref inRangeList);
					Profiler.EndSample();
					if (inRangeList.Length == 0)
						return;
					for (int j = 0; j < 6; j++)
					{
						var targetEntity = physWorld.Bodies[inRangeList[i % inRangeList.Length]].Entity;
						var target = centerOfMass[targetEntity].Value;
						if (math.lengthsq(target - t.Value) <= core.targetingRangeSq)
							curTargets[i] = target;
					}
					FireBurst(chunkIndex, t.Value, baseAngle, ref curTargets, core, faction);
					core.nextVolleyTime = elapsed + core.fireRate;
					cores[i] = core;
				}
			}
			inRangeList.Dispose();
			curTargets.Dispose();
			Profiler.EndSample();
		}

		private void FireBurst(int cIndex, float3 startPos, float baseAngle, ref NativeArray<float3> targets, PhosCore core, FactionId team)
		{
			for (int i = 0; i < 6; i++)
			{
				FirePorjectile(cIndex, startPos, baseAngle + (math.PI / 3) * i, targets[i % targets.Length], core, elapsed + core.targetDelay + (i * (1 / 12f)), team);
			}
		}

		private void FirePorjectile(int cIndex, float3 startPos, float angle, float3 target, PhosCore core, double targetTime, FactionId team)
		{
			var dir = math.rotate(quaternion.RotateY(angle), Vector3.forward);
			var pos = startPos + (dir * 2.9f) + new float3(0, 4, 0);
			dir.y = .4f;
			var vel = dir * core.projectileSpeed;
			var proj = cmb.Instantiate(cIndex, projectile);
			cmb.SetComponent(cIndex, proj, new PhysicsVelocity { Linear = vel });
			cmb.SetComponent(cIndex, proj, new Translation { Value = pos });
			cmb.SetComponent(cIndex, proj, team);
			cmb.AddComponent(cIndex, proj, new DeathTime { Value = elapsed + 15 });
			cmb.AddComponent(cIndex, proj, new PhosProjectile
			{ 
				flightSpeed = core.projectileSpeed * 15,
				target = target,
				targetTime = targetTime
			});
		}
	}


	protected override void OnCreate()
	{
		base.OnCreate();
		GameEvents.OnMapLoaded += Init;
		buildPhysics = World.GetOrCreateSystem<BuildPhysicsWorld>();
		var desc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				typeof(PhosCore),
				ComponentType.ReadOnly<PhosCoreData>(),
				ComponentType.ReadOnly<Translation>(),
				ComponentType.ReadOnly<FactionId>(),
			}
		};
		var projLoad = Addressables.LoadAssetAsync<ProjectileMeshEntity>("EnemyProjectile");
		var laserLoad = Addressables.LoadAssetAsync<ProjectileMeshEntity>("EnemyLaser");
		projLoad.Completed += r =>
		{
			if (r.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
			{
				projectile = r.Result;
				projectileEntity = projectile.GetEntity();
				_state = 1;
			}
		};
		laserLoad.Completed += r =>
		{
			if (r.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
				laser = r.Result;
		};
		_entityQuery = GetEntityQuery(desc);
	}

	protected void Init()
	{
		_map = Map.ActiveMap;
		_inRangeList = new NativeList<int>(Allocator.Persistent);
		_curTargets = new NativeArray<float3>(6, Allocator.Persistent);
		GameEvents.OnMapLoaded -= Init;
		_endSimulation = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}


	protected override void OnDestroy()
	{
		base.OnDestroy();
		_inRangeList.Dispose();
		_curTargets.Dispose();
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		switch (_state)
		{
			case 0:
				break;

			case 1:
				inputDeps = SimulateAI(inputDeps);
				break;
		}
		return inputDeps;
	}

	private JobHandle SimulateAI(JobHandle handle)
	{
		var phosJob = new PhosTargetingJob
		{
			dataType = GetArchetypeChunkComponentType<PhosCoreData>(true),
			coreType = GetArchetypeChunkComponentType<PhosCore>(false),
			factionType = GetArchetypeChunkComponentType<FactionId>(true),
			translationType = GetArchetypeChunkComponentType<Translation>(true),
			physWorld = buildPhysics.PhysicsWorld,
			colWorld = buildPhysics.PhysicsWorld.CollisionWorld,
			cmb = _endSimulation.CreateCommandBuffer().ToConcurrent(),
			centerOfMass = GetComponentDataFromEntity<CenterOfMass>(true),
			elapsed = Time.DeltaTime,
			projectile = projectileEntity
		};

		handle = phosJob.Schedule(_entityQuery, handle);
		return handle;
	}
}

[BurstCompile]
[UpdateAfter(typeof(PhosCoreSystem))]
public class PhosProjectileSystem : JobComponentSystem
{
	[BurstCompile]
	private struct PhosProjectileJob : IJobChunk
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
				typeof(Disabled),
				typeof(FrozenRenderSceneTag)
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