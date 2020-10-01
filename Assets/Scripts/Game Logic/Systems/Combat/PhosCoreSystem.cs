using Amatsugu.Phos;

using System.Linq;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine;

[BurstCompile]
[UpdateBefore(typeof(BuildPhysicsWorld))]
public class PhosCoreSystem : ComponentSystem
{
	private int _state = 0;
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
		_inRangeList = new NativeList<int>(Allocator.Persistent);
		_curTargets = new NativeArray<float3>(6, Allocator.Persistent);
		GameEvents.OnMapLoaded -= Init;
		_state = 1;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_state == 0)
			return;
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
		Entities.WithNone<BuildingOffTag>().ForEach((Entity e, ref PhosCore core, ref PhosCoreData data, ref Translation t, ref FactionId faction) =>
		{
			var baseAngle = (((float)Time.ElapsedTime % core.spinRate) / core.spinRate) * (math.PI * 2); //Rotation of the ring
			PostUpdateCommands.SetComponent(data.ring, new Rotation { Value = quaternion.AxisAngle(Vector3.up, baseAngle + (math.PI * 2) / 12f) });
			if (core.nextVolleyTime <= Time.ElapsedTime)
			{
				_inRangeList.Clear();
				buildPhysics.AABBCast(t.Value, new float3(core.targetingRange), new CollisionFilter
				{
					BelongsTo = (uint)faction.Value.AsCollisionLayer(),
					CollidesWith = ~(uint)(faction.Value.AsCollisionLayer() | CollisionLayer.Projectile),//~((1u << (int)faction.Value) | (1u << (int)Faction.None) | (1u << (int)Faction.PlayerProjectile) | (1u << (int)Faction.PhosProjectile) | (1u << (int)Faction.Tile) | (1u << (int)Faction.Unit)),
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
		if (target.Equals(0))
			return;
		var dir = math.rotate(quaternion.RotateY(angle), Vector3.forward);
		var pos = startPos + (dir * 2.9f) + new float3(0, 4, 0);
		dir.y = .4f;
		var vel = dir * core.projectileSpeed;
		/*var proj = projectile.BufferedInstantiate(PostUpdateCommands, pos, .5f, vel);
		PostUpdateCommands.AddComponent(proj, new DeathTime { Value = Time.ElapsedTime + 15 });

		PostUpdateCommands.AddComponent(proj, new PhosProjectile
		{
			targetTime = targetTime,
			target = target,
			flightSpeed = core.projectileSpeed * 15
		});
		*/
	}
}

[BurstCompile]
[UpdateAfter(typeof(PhosCoreSystem))]
public class PhosProjectileSystem : JobComponentSystem
{
	[BurstCompile]
	private struct PhosProjectileJob : IJobChunk //IJobForEachWithEntity<PhosProjectile, PhysicsVelocity, Translation>
	{
		public double curTime;
		[ReadOnly] public ComponentTypeHandle<PhosProjectile> projectileType;
		public ComponentTypeHandle<PhysicsVelocity> velocityType;
		[ReadOnly] public ComponentTypeHandle<Translation> translationType;
		[ReadOnly] public EntityTypeHandle entityType;
		public EntityCommandBuffer.ParallelWriter CMB;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var vel = chunk.GetNativeArray(velocityType);
			var proj = chunk.GetNativeArray(projectileType);
			var trans = chunk.GetNativeArray(translationType);
			var entities = chunk.GetNativeArray(entityType);
			for (int i = 0; i < chunk.Count; i++)
			{
				if (curTime < proj[i].targetTime)
					continue;
				var v = vel[i];
				v.Linear = math.normalize(proj[i].target - trans[i].Value) * proj[i].flightSpeed;
				vel[i] = v;
				CMB.RemoveComponent<PhosProjectile>(chunkIndex, entities[i]);
			}
		}
	}

	private EntityQuery entityQuery;
	private EndSimulationEntityCommandBufferSystem endSimulation;

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
		endSimulation = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var job = new PhosProjectileJob
		{
			curTime = Time.ElapsedTime,
			projectileType = GetComponentTypeHandle<PhosProjectile>(true),
			velocityType = GetComponentTypeHandle<PhysicsVelocity>(false),
			translationType = GetComponentTypeHandle<Translation>(true),
			CMB = endSimulation.CreateCommandBuffer().AsParallelWriter(),
			entityType = GetEntityTypeHandle()
		};
		inputDeps = job.ScheduleParallel(entityQuery, inputDeps);
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