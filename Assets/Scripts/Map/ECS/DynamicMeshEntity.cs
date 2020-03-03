using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using SphereCollider = Unity.Physics.SphereCollider;

[CreateAssetMenu(menuName = "ECS/Dynamic Entity")]
public class DynamicMeshEntity : MeshEntityRotatable
{
	[Header("Collision")]
	public float colliderRadius;

	public override IEnumerable<ComponentType> GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[]
		{
			typeof(PhysicsCollider),
			typeof(PhysicsVelocity),
			typeof(PhysicsMass)
		});

	}

	public virtual Entity Instantiate(float3 position, quaternion rotation, float3 velocity = default, float3 angularVelocity = default)
	{
		var e = Instantiate(position, 1, rotation);
		var em = World.DefaultGameObjectInjectionWorld.EntityManager;
		em.SetComponentData(e, GetCollider());
		em.SetComponentData(e, new PhysicsVelocity
		{
			Linear = velocity,
			Angular = angularVelocity
		});
		em.AddComponentData(e, PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 1));

		return e;
	}

	public Entity BufferedInstantiate(EntityCommandBuffer commandBuffer, Vector3 position, Quaternion rotation, float3 velocity = default, float3 angularVelocity = default)
	{
		var e = BufferedInstantiate(commandBuffer, position, 1, rotation);
		var col = GetCollider();
		commandBuffer.SetComponent(e, col);
		commandBuffer.SetComponent(e, new PhysicsVelocity
		{
			Linear = velocity,
			Angular = angularVelocity
		});
		commandBuffer.SetComponent(e, PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 1));
		return e;
	}

	protected virtual PhysicsCollider GetCollider() => new PhysicsCollider
	{
		Value = SphereCollider.Create(new SphereGeometry
		{
			Radius = colliderRadius,
		}, CollisionFilter.Default, new Unity.Physics.Material
		{
			Flags = Unity.Physics.Material.MaterialFlags.EnableCollisionEvents
		})
	};
}
