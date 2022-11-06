using Amatsugu.Phos;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

using UnityEngine;

using SphereCollider = Unity.Physics.SphereCollider;

[CreateAssetMenu(menuName = "ECS/Dynamic Entity")]
[Obsolete]
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
		GameRegistry.EntityManager.SetComponentData(entity, new PhysicsGravityFactor
		{
			Value = gravity ? gravityFactor : 0
		});
		GameRegistry.EntityManager.AddComponentData(entity, GetMass());
		GameRegistry.EntityManager.SetComponentData(entity, GetCollider());

	}

	protected override void PrepareComponentData(Entity entity, EntityCommandBuffer postUpdateCommands)
	{
		base.PrepareComponentData(entity, postUpdateCommands);
		GameRegistry.EntityManager.AddComponentData(entity, new PhysicsGravityFactor
		{
			Value = gravity ? gravityFactor : 0
		});
		GameRegistry.EntityManager.AddComponentData(entity, GetMass());
		GameRegistry.EntityManager.AddComponentData(entity, GetCollider());
	}

	public virtual Entity Instantiate(float3 position, quaternion rotation, float scale = 1, float3 velocity = default, float3 angularVelocity = default)
	{
		var e = Instantiate(position, scale, rotation);
		var em = GameRegistry.EntityManager;
		em.SetComponentData(e, new PhysicsVelocity
		{
			Linear = velocity,
			Angular = angularVelocity
		});
		return e;
	}

	public Entity BufferedInstantiate(EntityCommandBuffer commandBuffer, Vector3 position, Quaternion rotation, float scale = 1, float3 velocity = default, float3 angularVelocity = default)
	{
		var e = BufferedInstantiate(commandBuffer, position, scale, rotation);
		commandBuffer.SetComponent(e, new PhysicsVelocity
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
			CollisionResponse = CollisionResponsePolicy.CollideRaiseCollisionEvents
		})
	};

	

	protected virtual CollisionFilter GetFilter() => CollisionFilter.Default;
}