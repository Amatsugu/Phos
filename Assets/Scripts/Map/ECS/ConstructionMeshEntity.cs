﻿using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine;

[CreateAssetMenu(menuName = "ECS/Construction Mesh Enity")]
public class ConstructionMeshEntity : ScriptableObject
{
	public Material material;

	public NativeArray<Entity> Instantiate(float3 pos, quaternion rotation, BuildingMeshEntity buildingMesh, float constructTime)
	{
		var arr = new NativeArray<Entity>(1 + buildingMesh.subMeshes.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

		var arch = Map.EM.CreateArchetype(typeof(RenderMesh),
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

		Map.EM.CreateEntity(arch, arr);

		Render(pos, rotation, buildingMesh, arr[0], buildingMesh.height, constructTime);

		for (int i = 1; i < buildingMesh.subMeshes.Length +1; i++)
		{
			var p = pos + math.rotate(rotation, buildingMesh.subMeshes[i - 1].offset);

			Render(p, rotation, buildingMesh.subMeshes[i-1].mesh, arr[i], buildingMesh.height, constructTime);
		}
		return arr;
	}

	private void Render(float3 pos, quaternion rotation, MeshEntityRotatable mesh, Entity entity, float height, float duration)
	{
		var renderMesh = new RenderMesh
		{
			castShadows = mesh.castShadows,
			receiveShadows =mesh.receiveShadows,
			material = material,
			mesh = mesh.mesh,
			subMesh = 0
		};

		Map.EM.SetSharedComponentData(entity, renderMesh);
		Map.EM.SetComponentData(entity, new ConstructionHeight { Value = height });
		Map.EM.SetComponentData(entity, new ConstructionOffset { Value = pos.y });
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
