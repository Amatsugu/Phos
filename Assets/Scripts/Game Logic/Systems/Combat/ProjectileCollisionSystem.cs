using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

[RequireComponentTag(TagComponents = new[] { typeof(FactionId) })]
//[BurstCompile]
public struct ProjectileCollisionJob : ICollisionEventsJob
{
	public ComponentDataFromEntity<Health> health;
	public ComponentDataFromEntity<FactionId> faction;
	public ComponentDataFromEntity<Damage> damage;
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
		//cmb.AddComponent(src.Index, src, new TimedDeathSystem.DeathTime { Value = time } ); //TODO: Collision Effect
		cmb.DestroyEntity(src.Index, src);
		if (!health.HasComponent(tgt))
			return;
		var h = health[tgt];
		h.Value -= damage[src].Value;
		health[tgt] = h;
	}
}

//[BurstCompile]
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
			cmb = _endSimSystem.CreateCommandBuffer().ToConcurrent(),
			time = Time.ElapsedTime
		};
		inputDeps = job.Schedule(_sim.Simulation, ref _physicsWorld.PhysicsWorld, inputDeps);
		return inputDeps;
	}
}