using Amatsugu.Phos;

using Unity.Collections;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

public static class PhysicsUtilz
{
	public static void AABBCast(this BuildPhysicsWorld world, float3 center, float3 extents, CollisionFilter filter, ref NativeList<int> castHits)
	{
		var bounds = new Aabb
		{
			Max = center + extents,
			Min = center - extents
		};
		AABBCast(world, bounds, filter, ref castHits);
	}

	public static void AABBCast(this BuildPhysicsWorld world, Aabb bounds, CollisionFilter filter, ref NativeList<int> castHits)
	{
		world.PhysicsWorld.CollisionWorld.OverlapAabb(new OverlapAabbInput
		{
			Aabb = bounds,
			Filter = filter
		}, ref castHits);
	}

	public static bool GetTileFromRay(this BuildPhysicsWorld world, UnityEngine.Ray ray, float dist, CollisionFilter filter, out HexCoords tilePos)
	{
		if (world.PhysicsWorld.CastRay(new RaycastInput
		{
			Start = ray.origin,
			End = ray.GetPoint(dist),
			Filter = filter
		}, out var hit))
		{
			tilePos = HexCoords.FromPosition(hit.Position);
			return true;
		}
		tilePos = default;
		return false;
	}

	public static bool GetTileFromRay(this BuildPhysicsWorld world, UnityEngine.Ray ray, float dist, out HexCoords tilePos)
	{
		return world.GetTileFromRay(ray, dist, new CollisionFilter
		{
			GroupIndex = 0,
			BelongsTo = (int)CollisionLayer.Tile,
			CollidesWith = (int)CollisionLayer.Tile
		}, out tilePos);
	}

	public static CollisionLayer AsCollisionLayer(this Faction faction)
	{
		return faction switch
		{
			Faction.Phos => CollisionLayer.Phos,
			Faction.Player => CollisionLayer.Player,
			Faction.None => CollisionLayer.Default,
			_ => throw new System.NotImplementedException($"Invalid faction: {faction}")
		};
	}

	public static float3 CalculateProjectileShotVector(float3 start, float3 tgt, float flightTime = 5f)
	{
		var d = math.length(start - tgt);
		var h = start.y - tgt.y;
		var g = 9.8f;
		var vY = ((g * flightTime * flightTime) - (2 * h)) / (2 * flightTime);
		var vX = d / flightTime;
		var v = new float3(0, vY, vX);

		var dir = start - tgt;
		dir.y = 0;
		var r = quaternion.LookRotation(-dir, math.up());
		v = math.rotate(r, v);
		return v;
	}
}