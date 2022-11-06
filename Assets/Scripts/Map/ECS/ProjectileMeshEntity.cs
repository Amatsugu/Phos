using Amatsugu.Phos;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

using UnityEngine;

[Obsolete]
public class ProjectileMeshEntity : PhysicsMeshEntity
{
	[Header("Projectile Settings")]
	public Faction faction;
	public bool friendlyFire;
	public float damage;
	public float3 scale = 1;

	public override IEnumerable<ComponentType> GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[] {
				typeof(Damage),
				typeof(FactionId),
			});
	}

	[Obsolete]
	public override void PrepareDefaultComponentData(Entity entity)
	{
		base.PrepareDefaultComponentData(entity);
		GameRegistry.EntityManager.SetComponentData(entity, new Damage
		{
			Value = damage,
			friendlyFire = friendlyFire
		});
		GameRegistry.EntityManager.SetComponentData(entity, new FactionId
		{
			Value = faction
		});
		if (nonUniformScale)
			GameRegistry.EntityManager.SetComponentData(entity, new NonUniformScale { Value = scale });
		else
			GameRegistry.EntityManager.SetComponentData(entity, new Scale { Value = scale.x });
	}

	protected override void PrepareComponentData(Entity entity, EntityCommandBuffer postUpdateCommands)
	{
		base.PrepareComponentData(entity, postUpdateCommands);
		GameRegistry.EntityManager.SetComponentData(entity, new Damage
		{
			Value = damage,
			friendlyFire = friendlyFire
		});
		GameRegistry.EntityManager.SetComponentData(entity, new FactionId
		{
			Value = faction
		});
		if (nonUniformScale)
			GameRegistry.EntityManager.SetComponentData(entity, new NonUniformScale { Value = scale });
		else
			GameRegistry.EntityManager.SetComponentData(entity, new Scale { Value = scale.x });
	}

	public static Entity ShootProjectile(EntityCommandBuffer cmb, Entity projectileEntity, float3 pos, float3 velocity, double deathTime)
	{
		var proj = cmb.Instantiate(projectileEntity);
		cmb.SetComponent(proj, new Translation { Value = pos });
		cmb.SetComponent(proj, new Rotation { Value = quaternion.LookRotation(velocity, math.up()) });
		cmb.AddComponent(proj, new DeathTime { Value = deathTime });
		cmb.SetComponent(proj, new PhysicsVelocity { Linear = velocity });
		cmb.RemoveComponent<Disabled>(proj);
		return proj;
	}

	protected override CollisionFilter GetFilter() => friendlyFire ? new CollisionFilter
	{
		BelongsTo = (uint)(faction.Invert().AsCollisionLayer() | faction.AsCollisionLayer()),
		CollidesWith = ~(uint)(CollisionLayer.Projectile | faction.Invert().AsCollisionLayer() | faction.AsCollisionLayer()),
		GroupIndex = 0
	} : new CollisionFilter
	{
		BelongsTo = (uint)(faction.Invert().AsCollisionLayer()),
		CollidesWith = ~(uint)(CollisionLayer.Projectile | faction.Invert().AsCollisionLayer()),
		GroupIndex = 0
	};
}