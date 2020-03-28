using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

[RequireComponentTag(TagComponents = new[] { typeof(FactionId) })]
[BurstCompile]
public struct ProjectileCollisionJob : ICollisionEventsJob
{
	public ComponentDataFromEntity<Health> health;
	public ComponentDataFromEntity<FactionId> faction;
	public ComponentDataFromEntity<Damage> damage;
	public ComponentDataFromEntity<DeathTime> deathTime;
	public EntityCommandBuffer.Concurrent cmb;
	public double time;

	public void Execute(CollisionEvent collisionEvent)
	{
		if (damage.HasComponent(collisionEvent.Entities.EntityA))
			DealDamage(collisionEvent.Entities.EntityA, collisionEvent.Entities.EntityB);
		else if (damage.HasComponent(collisionEvent.Entities.EntityB))
			DealDamage(collisionEvent.Entities.EntityB, collisionEvent.Entities.EntityA);
	}

	private void DealDamage(Entity src, Entity tgt)
	{
		//deathTime[src] = new DeathTime { Value = time };
		if (!health.HasComponent(tgt))
			return;
		var dmg = damage[src];
		if (!dmg.friendlyFire && faction.HasComponent(src) && faction.HasComponent(tgt) && faction[src] == faction[tgt])
			return;
		var h = health[tgt];
		h.Value -= dmg.Value;
		health[tgt] = h;
	}
}

[BurstCompile]
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
			cmb = _endSimSystem.CreateCommandBuffer().ToConcurrent(),
			time = Time.ElapsedTime
		};
		inputDeps = job.Schedule(_sim.Simulation, ref _physicsWorld.PhysicsWorld, inputDeps);
		return inputDeps;
	}
}