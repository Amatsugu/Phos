using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile Decorators/Mesh")]
public class MeshDecorator : TileDecorator
{
	public Vector3 rotation;
	public Vector3 offset;

	public override int GetDecorEntityCount(Tile tile)
	{
		return 1;
	}

	public override Entity[] Render(Tile tile, Entity parent)
	{
		var rot = rotation;
		rot.y = Mathf.PerlinNoise(tile.Coords.worldX / 10f, tile.Coords.worldZ / 10f) * 360f;
		var e = meshEntity.Instantiate(tile.SurfacePoint + offset, Vector3.one, Quaternion.Euler(rot));
		return new Entity[] { e };
	}

	public override void UpdateHeight(NativeSlice<Entity> decor, Tile tile, Entity parent)
	{
		foreach (var tileDecor in decor)
		{
			var p = Map.EM.GetComponentData<Translation>(tileDecor);
			p.Value.y = tile.Height + offset.y;
			Map.EM.SetComponentData(tileDecor, p);
		}
	}
}
