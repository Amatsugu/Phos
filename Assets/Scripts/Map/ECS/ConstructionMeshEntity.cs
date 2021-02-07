using Amatsugu.Phos;
using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Units;

using System;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "ECS/Construction Mesh Enity")]
public class ConstructionMeshEntity : ScriptableObject
{
	public Material material;

	public void Instantiate(float3 pos, quaternion rotation, float height, BuildingMeshEntity buildingMesh, float constructTime)
	{
		var arr = new NativeArray<Entity>(1 + buildingMesh.subMeshes.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

		var arch = GetArchetype();

		Map.EM.CreateEntity(arch, arr);

		Render(pos, rotation, buildingMesh, arr[0], height, constructTime);

		for (int i = 1; i < buildingMesh.subMeshes.Length + 1; i++)
		{
			//var p = pos + math.rotate(rotation, buildingMesh.subMeshes[i - 1].offset);

			Render(buildingMesh.subMeshes[i - 1].offset, rotation, buildingMesh.subMeshes[i - 1].mesh, arr[i], height, constructTime, buildingMesh.subMeshes[i - 1].offset.y);
		}
		for (int i = 0; i < buildingMesh.subMeshes.Length; i++)
		{
			Map.EM.AddComponent<LocalToParent>(arr[i + 1]); ;
			if (buildingMesh.subMeshes[i].parent.id == -1 || buildingMesh.subMeshes[i].parent.id == i)
				Map.EM.AddComponentData(arr[i + 1], new Parent { Value = arr[0] });
			else
				Map.EM.AddComponentData(arr[i + 1], new Parent { Value = arr[buildingMesh.subMeshes[i].parent.id + 1] });
		}
		arr.Dispose();
	}

	public void Instantiate(float3 pos, quaternion rotation, float height, float constructionTime, params MeshEntityRotatable[] meshes)
	{
		var arr = new NativeArray<Entity>(meshes.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

		var arch = GetArchetype();

		Map.EM.CreateEntity(arch, arr);

		for (int i = 0; i < meshes.Length; i++)
		{
			Render(pos, rotation, meshes[i], arr[i], height, constructionTime);
		}
		arr.Dispose();
	}

	public void Instantiate(float3 pos, quaternion rotation, MobileUnitEntity unitEntity, float height, float constructTime)
	{
		var arr = new NativeArray<Entity>(1 + unitEntity.subMeshes.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

		var arch = GetArchetype();

		Map.EM.CreateEntity(arch, arr);

		Render(pos, rotation, unitEntity, arr[0], height, constructTime);

		for (int i = 1; i < unitEntity.subMeshes.Length + 1; i++)
		{
			//var p = pos + math.rotate(rotation, unitEntity.subMeshes[i - 1].offset);

			Render(unitEntity.subMeshes[i - 1].offset, rotation, unitEntity.subMeshes[i - 1].mesh, arr[i], height, constructTime, unitEntity.subMeshes[i - 1].offset.y);
		}
		for (int i = 0; i < unitEntity.subMeshes.Length; i++)
		{
			Map.EM.AddComponent<LocalToParent>(arr[i + 1]); ;
			if(unitEntity.subMeshes[i].parent.id == -1 || unitEntity.subMeshes[i].parent.id == i)
				Map.EM.AddComponentData(arr[i + 1], new Parent { Value = arr[0] });
			else
				Map.EM.AddComponentData(arr[i + 1], new Parent { Value = arr[unitEntity.subMeshes[i].parent.id + 1] });
		}
		arr.Dispose();
	}

	public EntityArchetype GetArchetype()
	{
		return Map.EM.CreateArchetype(typeof(RenderMesh),
			typeof(Translation),
			typeof(Scale),
			typeof(Rotation),
			typeof(DeathTime),
			typeof(LocalToWorld),
			typeof(PerInstanceCullingTag),
			typeof(WorldRenderBounds),
			typeof(ChunkWorldRenderBounds),
			typeof(RenderBounds),
			typeof(ConstructionOffset),
			typeof(ConstructionStart),
			typeof(ConstructionDuration),
			typeof(ConstructionHeight));
	}

	private void Render(float3 pos, quaternion rotation, MeshEntityRotatable mesh, Entity entity, float height, float duration, float offset = 0)
	{
		//var mat = new Material(material);
		//mat.SetFloat("Height", height);
		//mat.SetFloat("Offset", pos.y - offset);
		//mat.SetFloat("StartTime", Time.time);
		//mat.SetFloat("Duration", duration);

		var renderMesh = new RenderMesh
		{
			castShadows = mesh.castShadows,
			receiveShadows = mesh.receiveShadows,
			material = material,
			mesh = mesh.mesh,
			subMesh = 0
		};

		//get
		Map.EM.SetSharedComponentData(entity, renderMesh);

		Map.EM.SetComponentData(entity, new ConstructionHeight { Value = height });
		Map.EM.SetComponentData(entity, new ConstructionOffset { Value = pos.y - offset });
		Map.EM.SetComponentData(entity, new ConstructionStart { Value = Time.time });
		Map.EM.SetComponentData(entity, new ConstructionDuration { Value = duration });
		Map.EM.SetComponentData(entity, new Translation { Value = pos });
		Map.EM.SetComponentData(entity, new Rotation { Value = rotation });
		Map.EM.SetComponentData(entity, new Scale { Value = 1 });
		Map.EM.SetComponentData(entity, new DeathTime { Value = Time.time + duration });

		Map.EM.SetComponentData(entity, new RenderBounds
		{
			Value = new AABB
			{
				Center = mesh.mesh.bounds.center,
				Extents = mesh.mesh.bounds.extents
			}
		});
	}
}