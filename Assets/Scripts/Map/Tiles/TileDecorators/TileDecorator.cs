using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;


public abstract class TileDecorator : ScriptableObject
{
	public MeshEntity meshEntity;

    public abstract int GetDecorEntityCount(Tile tile);

	public abstract Entity[] Render(Tile tile, Entity parent);

	public virtual void UpdateHeight(NativeSlice<Entity> decor, Tile tile, Entity parent)
	{
		foreach (var tileDecor in decor)
		{
			var p = Map.EM.GetComponentData<Translation>(tileDecor);
			p.Value.y = tile.Height;
			Map.EM.SetComponentData(tileDecor, p);
		}
	}
}
