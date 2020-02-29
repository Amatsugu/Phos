using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Physics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;

[RequireComponentTag(TagComponents = new[] { typeof(FactionId) })]
[BurstCompile]
public struct ProjectileCollisionJob : ICollisionEventsJob
{
	public ComponentDataFromEntity<Health> health;
	public ComponentDataFromEntity<FactionId> faction;
	public ComponentDataFromEntity<Damage> damage;

	public void Execute(CollisionEvent collisionEvent)
	{
		if (damage.HasComponent(collisionEvent.Entities.EntityA))
			DealDamage(collisionEvent.Entities.EntityA, collisionEvent.Entities.EntityB);
		else
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
	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var job = new ProjectileCollisionJob
		{
			damage = GetComponentDataFromEntity<Damage>(true),
			health = GetComponentDataFromEntity<Health>(false),
			faction = GetComponentDataFromEntity<FactionId>(true),
		};


		return inputDeps;
	}
}