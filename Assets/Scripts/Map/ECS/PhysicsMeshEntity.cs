using System.Collections.Generic;
using System.Linq;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

using UnityEngine;

using SphereCollider = Unity.Physics.SphereCollider;

[CreateAssetMenu(menuName = "ECS/Dynamic Entity")]
public class PhysicsMeshEntity : MeshEntityRotatable
{
	[Header("Physics")]
	public bool enableCollision = true;
	[ConditionalHide("enableCollision")]
	public float colliderRadius;
	public bool isKenematic;
	public bool gravity = true;
	[ConditionalHide("gravity")]
	public float gravityFactor = 1;
	public float mass = 1;

	public override IEnumerable<ComponentType> GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[]
		{
			typeof(PhysicsCollider),
			typeof(PhysicsVelocity),
			typeof(PhysicsMass),
			typeof(PhysicsGravityFactor)
		});
	}

	public override void PrepareDefaultComponentData(Entity entity)
	{
		base.PrepareDefaultComponentData(entity);
		Map.EM.SetComponentData(entity, new PhysicsGravityFactor
		{
			Value = gravity ? gravityFactor : 0
		});
		Map.EM.AddComponentData(entity, GetMass());
		Map.EM.SetComponentData(entity, GetCollider());

	}

	public virtual Entity Instantiate(float3 position, quaternion rotation, float3 scale, float3 velocity = default, float3 angularVelocity = default)
	{
		var e = Instantiate(position, scale, rotation);
		var em = Map.EM;
		em.SetComponentData(e, new PhysicsVelocity
		{
			Linear = velocity,
			Angular = angularVelocity
		});
		return e;
	}

	public Entity BufferedInstantiate(EntityCommandBuffer commandBuffer, Vector3 position, Quaternion rotation, float3 scale, float3 velocity = default, float3 angularVelocity = default)
	{
		var e = BufferedInstantiate(commandBuffer, position, scale, rotation);
		commandBuffer.SetComponent(e, new PhysicsVelocity
		{
			Linear = velocity,
			Angular = angularVelocity
		});
		return e;
	}

	public Entity ConcurrentInstantiate(EntityCommandBuffer.Concurrent commandBuffer, int jobIndex, Vector3 position, Quaternion rotation, float3 scale, float3 velocity = default, float3 angularVelocity = default)
	{
		var e = ConcurrentInstantiate(commandBuffer, jobIndex, position, scale, rotation);
		commandBuffer.SetComponent(jobIndex, e, new PhysicsVelocity
		{
			Linear = velocity,
			Angular = angularVelocity
		});
		return e;
	}

	protected virtual PhysicsMass GetMass() => isKenematic ? PhysicsMass.CreateKinematic(MassProperties.UnitSphere) : PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 1);

	protected virtual PhysicsCollider GetCollider() => new PhysicsCollider
	{
		Value = SphereCollider.Create(new SphereGeometry
		{
			Radius = colliderRadius,
		}, GetFilter(), new Unity.Physics.Material
		{
			Flags = Unity.Physics.Material.MaterialFlags.EnableCollisionEvents
		})
	};

	protected virtual CollisionFilter GetFilter() => CollisionFilter.Default;
}