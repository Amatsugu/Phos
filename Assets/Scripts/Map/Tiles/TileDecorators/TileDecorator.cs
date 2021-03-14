using Amatsugu.Phos;
using Amatsugu.Phos.Tiles;

using System;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

using UnityEngine;

[Serializable]
public abstract class TileDecorator : ScriptableObject
{
	[Obsolete]
	[CreateNewAsset("Assets/GameData/MapAssets/Meshes/Decor", typeof(MeshEntityRotatable))]
	public MeshEntityRotatable meshEntity;
	public GameObject basePrefab;

	public float densityMulti = 1;

	public abstract int GetDecorEntityCount(Tile tile);

	public abstract void Render(Tile tile, NativeSlice<Entity> decor);

	[Obsolete]
	public virtual void UpdateHeight(NativeSlice<Entity> decor, Tile tile)
	{
		foreach (var tileDecor in decor)
		{
			var p = Map.EM.GetComponentData<Translation>(tileDecor);
			p.Value.y = tile.Height;
			Map.EM.SetComponentData(tileDecor, p);
		}
	}

	public virtual void DeclarePrefabs(List<GameObject> prefabs)
	{
		prefabs.Add(basePrefab);
	}
}