using Amatsugu.Phos.Tiles;

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


namespace Amatsugu.Phos.TileEntities
{
	[CreateAssetMenu(menuName = "Map Asset/Tile/Tile Info")]
	[Serializable]
	public class TileEntity : MeshEntityRotatable, ISerializationCallbackReceiver
	{
		[Header("Tile Info")]
		public string description;
		public TileDecorator[] decorators;
		public bool isTraverseable = true;
		[HideInInspector]
		public string assetGuid;

		public override IEnumerable<ComponentType> GetComponents()
		{
			nonUniformScale = false;
			return base.GetComponents().Concat(new ComponentType[]{
			typeof(HexPosition),
			typeof(PhysicsCollider),
			typeof(PhysicsDebugDisplayData),
		});
		}

		public override void PrepareDefaultComponentData(Entity entity)
		{
			base.PrepareDefaultComponentData(entity);
			//Map.EM.SetComponentData(entity, new FactionId { Value = faction });

			Map.EM.SetComponentData(entity, new PhysicsDebugDisplayData
			{
				DrawColliders = 1
			});
			var physMat = Unity.Physics.Material.Default;
			physMat.Flags |= Unity.Physics.Material.MaterialFlags.EnableCollisionEvents;
			var colFilter = new CollisionFilter
			{
				CollidesWith = ~0u,
				BelongsTo = (1u << (int)Faction.Tile),
				GroupIndex = 0
			};
#if true
			var verts = new NativeArray<float3>(mesh.vertices.Select(v => (float3)v).ToArray(), Allocator.Temp);
			var collider = ConvexCollider.Create(verts, new ConvexHullGenerationParameters
			{
				BevelRadius = 0
			}, colFilter, physMat); ;
			verts.Dispose();
#else
		var collider = BoxCollider.Create(new BoxGeometry
		{
			BevelRadius = 0,
			Center = new float3(0, -25, 0),
			Size = new float3(1, 50, 1),
			Orientation = quaternion.identity
		}, colFilter, physMat);
		/*var collider = CylinderCollider.Create(new CylinderGeometry
		{
			BevelRadius = 0.04f,
			Center = new float3(0, -25, 0),
			SideCount = 6,
			Height = 50,
			Radius = pos.edgeLength,
			Orientation = quaternion.RotateX(math.radians(90))
		}, colFilter, physMat);*/
#endif

			Map.EM.SetComponentData(entity, new PhysicsCollider
			{
				Value = collider
			});
		}

		public virtual Entity Instantiate(HexCoords pos, float height)
		{
			var e = Instantiate(new float3(pos.WorldPos.x, height, pos.WorldPos.z), pos.edgeLength);
			Map.EM.SetComponentData(e, new HexPosition { Value = pos });

			return e;
		}

		public virtual Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return new Tile(pos, height, map, this);
		}
		public void OnBeforeSerialize()
		{
#if DEBUG
			assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this));
#endif
		}

		public virtual string GetNameString()
		{
			return name;
		}

		public void OnAfterDeserialize()
		{
		}
	}
}