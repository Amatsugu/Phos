using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
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
		rot.y = Mathf.PerlinNoise(tile.Coords.offsetX/ 50f, tile.Coords.offsetZ/ 50f) * 180f;
		var e = meshEntity.Instantiate(tile.SurfacePoint + offset, Vector3.one, Quaternion.Euler(rot));
		return new Entity[] { e };
	}
}
