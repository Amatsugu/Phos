using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;

[RequireComponentTag(TagComponents = new[] { typeof(FactionId) })]
[BurstCompile]
public struct ProjectileCollisionJob : ICollisionEventsJob
{
	public ComponentDataFromEntity<Health> health;
	[ReadOnly]
	public ComponentDataFromEntity<FactionId> faction;
	[ReadOnly]
	public ComponentDataFromEntity<Damage> damage;
	public ComponentDataFromEntity<DeathTime> deathTime;
	public EntityCommandBuffer.ParallelWriter cmb;
	public double time;

	public void Execute(CollisionEvent collisionEvent)
	{
		if (damage.HasComponent(collisionEvent.EntityA))
			DealDamage(collisionEvent.EntityA, collisionEvent.EntityB);
		else if (damage.HasComponent(collisionEvent.EntityB))
			DealDamage(collisionEvent.EntityB, collisionEvent.EntityA);
	}

	private void DealDamage(Entity src, Entity tgt)
	{
		deathTime[src] = new DeathTime { Value = time };
		//cmb.AddComponent(src.Index, src, ComponentType.ReadWrite<FrozenRenderSceneTag>());
		cmb.RemoveComponent<PhysicsVelocity>(src.Index, src);
		//cmb.DestroyEntity(src.Index, src);
		if (!health.HasComponent(tgt))
			return;
		var dmg = damage[src];
		if (!dmg.friendlyFire && faction.HasComponent(src) && faction.HasComponent(tgt))
		{
			var srcF = faction[src].Value;
			var tgtF = faction[tgt].Value;
			switch (srcF)
			{
				case Faction.Phos:
					if (tgtF == Faction.Phos)
						return;
					break;
				case Faction.Player:
					if (tgtF == Faction.Player)
						return;
					break;
			}
		}
		var h = health[tgt];
		h.Value -= dmg.Value;
		health[tgt] = h;
	}
}

[BurstCompile]
[UpdateBefore(typeof(TimedDeathSystem))]
public class ProjectileCollisionSystem : JobComponentSystem
{
	private BuildPhysicsWorld _physicsWorld;
	private StepPhysicsWorld _sim;
	private EndSimulationEntityCommandBufferSystem _endSimSystem;

	protected override void OnStartRunning()
	{
		_physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
		_sim = World.GetOrCreateSystem<StepPhysicsWorld>();
		_endSimSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var job = new ProjectileCollisionJob
		{
			damage = GetComponentDataFromEntity<Damage>(true),
			health = GetComponentDataFromEntity<Health>(false),
			faction = GetComponentDataFromEntity<FactionId>(true),
			deathTime = GetComponentDataFromEntity<DeathTime>(false),
			cmb = _endSimSystem.CreateCommandBuffer().AsParallelWriter(),
			time = Time.ElapsedTime
		};
		inputDeps = job.Schedule(_sim.Simulation, ref _physicsWorld.PhysicsWorld, inputDeps);
		inputDeps.Complete();
		return inputDeps;
	}
}