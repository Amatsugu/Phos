using Amatsugu.Phos;

using System.Collections.Generic;
using System.Linq;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

using UnityEngine;

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

	public override void PrepareDefaultComponentData(Entity entity)
	{
		base.PrepareDefaultComponentData(entity);
		Map.EM.SetComponentData(entity, new Damage
		{
			Value = damage,
			friendlyFire = friendlyFire
		});
		Map.EM.SetComponentData(entity, new FactionId
		{
			Value = faction
		});
		if (nonUniformScale)
			Map.EM.SetComponentData(entity, new NonUniformScale { Value = scale });
		else
			Map.EM.SetComponentData(entity, new Scale { Value = scale.x });
	}

	public Entity Instantiate(float3 position, float scale, float3 velocity = default, float3 angularVelocity = default)
	{
		var rot = (velocity.Equals(default) ? quaternion.identity : quaternion.LookRotation(velocity, new float3(0, 1, 0)));
		var e = Instantiate(position, rot, scale, velocity, angularVelocity);
		return e;
	}

	public Entity BufferedInstantiate(EntityCommandBuffer cmb, float3 position, float scale, float3 velocity = default, float3 angularVelocity = default)
	{
		var rot = (velocity.Equals(default) ? quaternion.identity : quaternion.LookRotation(velocity, new float3(0, 1, 0)));
		var e = BufferedInstantiate(cmb, position, rot, scale, velocity, angularVelocity);
		return e;
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
		CollidesWith = ~(uint)CollisionLayer.Projectile,
		GroupIndex = 0
	} : new CollisionFilter
	{
		BelongsTo = (uint)(faction.Invert().AsCollisionLayer()),
		CollidesWith = ~(uint)CollisionLayer.Projectile,
		GroupIndex = 0
	};
}