using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

using UnityEngine;

[Serializable]
public abstract class TileDecorator : ScriptableObject
{
	[CreateNewAsset("Assets/GameData/MapAssets/Meshes/Decor", typeof(MeshEntityRotatable))]
	public MeshEntityRotatable meshEntity;

	public float densityMulti = 1;

	public abstract int GetDecorEntityCount(Tile tile);

	public abstract Entity[] Render(Tile tile);

	public virtual void UpdateHeight(NativeSlice<Entity> decor, Tile tile)
	{
		foreach (var tileDecor in decor)
		{
			var p = Map.EM.GetComponentData<Translation>(tileDecor);
			p.Value.y = tile.Height;
			Map.EM.SetComponentData(tileDecor, p);
		}
	}
}