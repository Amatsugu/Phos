using Amatsugu.Phos.ECS;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

using UnityEngine;

using CapsuleCollider = Unity.Physics.CapsuleCollider;

namespace Amatsugu.Phos.TileEntities
{
	[Serializable]
	public class BuildingMeshEntity : MeshEntityRotatable
	{
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
			Map.EM.SetComponentData(e, new BuildingId { Value = id });
			Map.EM.SetComponentData(e, new FactionId { Value = faction });
			Map.EM.RemoveComponent<BuildingOffTag>(e);
			var col = CapsuleCollider.Create(new CapsuleGeometry
			{
				Radius = radius,
				Vertex0 = new float3(0, 0, 0),
				Vertex1 = new float3(0, height, 0)
			}, new CollisionFilter
			{
				BelongsTo = (uint)(faction.AsCollisionLayer() | CollisionLayer.Building),
				CollidesWith = ~(uint)(faction.Invert().AsCollisionLayer())
			});
			Map.EM.SetComponentData(e, new PhysicsCollider
			{
				Value = col
			});
			return e;
		}

		public NativeArray<Entity> InstantiateSubMeshes(float3 position, quaternion rotation, Entity parent)
		{
			var e = new NativeArray<Entity>(subMeshes.Length, Allocator.Persistent);
			for (int i = 0; i < subMeshes.Length; i++)
			{
				var pos = /*position +*/ math.rotate(rotation, subMeshes[i].offset);
				e[i] = subMeshes[i].mesh.Instantiate(pos, 1, rotation);
				Map.EM.AddComponent<LocalToParent>(e[i]);
				Map.EM.AddComponentData(e[i], new Parent { Value = parent });
			}
			return e;
		}
	}
}