using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

public static class PhysicsUtilz
{
	public static void AABBCast(this BuildPhysicsWorld world, float3 center, float3 extents, CollisionFilter filter, ref NativeList<int> castHits)
	{
		world.PhysicsWorld.CollisionWorld.OverlapAabb(new OverlapAabbInput
		{
			Aabb = new Aabb
			{
				Max = center + extents,
				Min = center - extents
			},
			Filter = filter
			
		}, ref castHits);
	}
}
