using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Physics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Physics.Systems;
using UnityEngine;

[RequireComponentTag(TagComponents = new[] { typeof(FactionId) })]
[BurstCompile]
public struct ProjectileCollisionJob : ICollisionEventsJob
{
	public ComponentDataFromEntity<Health> health;
	public ComponentDataFromEntity<FactionId> faction;
	public ComponentDataFromEntity<Damage> damage;
	public EntityCommandBuffer.Concurrent cmb;

	public void Execute(CollisionEvent collisionEvent)
	{
		if (damage.HasComponent(collisionEvent.Entities.EntityA))
			DealDamage(collisionEvent.Entities.EntityA, collisionEvent.Entities.EntityB);
		else if(damage.HasComponent(collisionEvent.Entities.EntityB))
			DealDamage(collisionEvent.Entities.EntityB, collisionEvent.Entities.EntityA);
	}

	private void DealDamage(Entity src, Entity tgt)
	{
		var h = health[tgt];
		h.Value -= damage[src].Value;
		health[tgt] = h;
	}
}

[BurstCompile]
public class ProjectileCollisionSystem : JobComponentSystem
{
	private PhysicsWorld _physicsWorld;
	private ISimulation _sim;

	protected override void OnStartRunning()
	{
		_physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld;
		_sim = World.GetOrCreateSystem<StepPhysicsWorld>().Simulation;
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var job = new ProjectileCollisionJob
		{
			damage = GetComponentDataFromEntity<Damage>(true),
			health = GetComponentDataFromEntity<Health>(false),
			faction = GetComponentDataFromEntity<FactionId>(true),
		};
		inputDeps = job.Schedule(_sim, ref _physicsWorld, inputDeps);
		return inputDeps;
	}
}