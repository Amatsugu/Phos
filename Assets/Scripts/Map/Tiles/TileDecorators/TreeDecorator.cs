using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Assest/Tile Decorators/Trees")]
public class TreeDecorator : TileDecorator
{
	public override int GetDecorEntityCount(Tile tile)
	{
		return Mathf.RoundToInt((tile.moisture + tile.temperature) / 2);
	}

	public override Entity[] Render(Tile tile, Entity parent)
	{
		var count = GetDecorEntityCount(tile);
		var entities = new Entity[count];
		for (int i = 0; i < count; i++)
		{
			var size = Random.Range(0.1f, .5f);
			var height = Random.Range(.5f, 2);
			var pos = Random.onUnitSphere * (tile.Coords.innerRadius - size);
			pos.y = height/2f;
			entities[i] = meshEntity.Instantiate(pos + tile.SurfacePoint, new Vector3(size, height, size));
		}
		return entities;

	}
}
