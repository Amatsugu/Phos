using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using CapsuleCollider = Unity.Physics.CapsuleCollider;

[Serializable]
public class BuildingMeshEntity : MeshEntityRotatable
{
	[Serializable]
	public struct SubMeshEntry
	{
		public float3 offset;
		public MeshEntityRotatable mesh;
	}

	[Header("Sub Mesh")]
	public SubMeshEntry[] subMeshes;
	[Header("Center of Mass")]
	public float3 centerOfMassOffset;
	[Header("Collider")]
	public float radius;
	public float height;

	public override IEnumerable<ComponentType> GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[]
		{
			typeof(Health),
			typeof(BuildingId),
			typeof(FactionId),
			typeof(BuildingOffTag),
			typeof(CenterOfMassOffset),
			typeof(CenterOfMass),
			typeof(PhysicsCollider),
			typeof(HexPosition)
		});
	}

	public override void PrepareDefaultComponentData(Entity entity)
	{
		base.PrepareDefaultComponentData(entity);
		Map.EM.SetComponentData(entity, new CenterOfMassOffset { Value = centerOfMassOffset });
	}

	public Entity Instantiate(float3 position, quaternion rotation, int id, float maxHealth, Faction faction)
	{
		var e = Instantiate(position, 1, rotation);
		Map.EM.SetComponentData(e, new Health
		{
			Value = maxHealth,
			maxHealth = maxHealth
		});
		Map.EM.SetComponentData(e, new BuildingId {	Value = id });
		Map.EM.SetComponentData(e, new FactionId { Value = faction });
		Map.EM.RemoveComponent<BuildingOffTag>(e);
		var col = CapsuleCollider.Create(new CapsuleGeometry
		{
			Radius = radius,
			Vertex0 = new float3(0, 0, 0),
			Vertex1 = new float3(0, height, 0)
		}, new CollisionFilter
		{
			BelongsTo = 1u << (int)Faction.Tile | 1u << (int)faction,
			CollidesWith = ~0u
		});
		Map.EM.SetComponentData(e, new PhysicsCollider
		{
			Value = col
		});
		return e;
	}

	public Entity[] InstantiateSubMeshes(float3 position, quaternion rotation)
	{
		var e = new Entity[subMeshes.Length];
		for (int i = 0; i < subMeshes.Length; i++)
		{
			var pos = position + math.rotate(rotation, subMeshes[i].offset);
			e[i] = subMeshes[i].mesh.Instantiate(pos, 1, rotation);
		}
		return e;
	}
}
