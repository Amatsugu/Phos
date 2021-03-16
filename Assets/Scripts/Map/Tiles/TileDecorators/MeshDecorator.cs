using Amatsugu.Phos;
using Amatsugu.Phos.Tiles;

using System;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile Decorators/Mesh")]
public class MeshDecorator : TileDecorator
{
	public bool multiMesh = false;
	[ConditionalHide("multiMesh")]
	public GameObject[] meshPrefabs;
	[ConditionalHide("multiMesh")]
	[Obsolete]
	public MeshEntityRotatable[] meshEntities;
	public Vector3 rotation;
	public float3 offset;
	public float randomRotMin = 0;
	public float randomRotMax = 180;

	public override int GetDecorEntityCount(Tile tile)
	{
		return 1;
	}

	public override void Instantiate(Entity tileInst, HexCoords coords, ref DynamicBuffer<GenericPrefab> genericPrefabs, EntityCommandBuffer postUpdateCommands)
	{
		//base.Instantiate(tileInst, genericPrefabs, postUpdateCommands);
		var rot = rotation;
		var mesh = basePrefab;
		if (multiMesh)
			mesh = meshPrefabs[UnityEngine.Random.Range(0, meshPrefabs.Length)];
		rot.y = Mathf.PerlinNoise(coords.WorldPos.x / 10f, coords.WorldPos.z / 10f).Remap(0, 1, randomRotMin, randomRotMax);
		var prefab = genericPrefabs[GameRegistry.PrefabDatabase[mesh]];

		var decor = postUpdateCommands.Instantiate(prefab.value);
		postUpdateCommands.AddComponent(decor, new Parent { Value = tileInst });
		postUpdateCommands.AddComponent<LocalToParent>(decor);
		postUpdateCommands.SetComponent(decor, new Rotation { Value = quaternion.Euler(rot) });
	}

	public override void DeclarePrefabs(List<GameObject> prefabs)
	{
		if (multiMesh)
			prefabs.AddRange(meshPrefabs);
		else
			prefabs.Add(basePrefab);
	}

	[Obsolete]
	public override void Render(Tile tile, NativeSlice<Entity> decor)
	{
		var rot = rotation;
		var mesh = meshEntity;
		if (multiMesh)
			mesh = meshEntities[UnityEngine.Random.Range(0, meshEntities.Length)];
		rot.y = Mathf.PerlinNoise(tile.Coords.WorldPos.x / 10f, tile.Coords.WorldPos.z / 10f).Remap(0, 1, randomRotMin, randomRotMax);
		decor[0] = mesh.Instantiate(tile.SurfacePoint + offset, Vector3.one, Quaternion.Euler(rot));
	}

	[Obsolete]
	public override void UpdateHeight(NativeSlice<Entity> decor, Tile tile)
	{
		foreach (var tileDecor in decor)
		{
			var p = Map.EM.GetComponentData<Translation>(tileDecor);
			p.Value.y = tile.Height + offset.y;
			Map.EM.SetComponentData(tileDecor, p);
		}
	}
}