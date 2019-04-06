using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile Decorators/Trees")]
public class TreeDecorator : TileDecorator
{
	public int minPerTile = 0;
	public int maxPerTile = 3;
	public float densityPower = 2;
	public float minHeight = .5f;
	public float maxHeight = 4;
	public float minSize = 0.1f;
	public float maxSize = .5f;


	public override int GetDecorEntityCount(Tile tile)
	{
		return Mathf.FloorToInt(Mathf.Clamp(Mathf.Pow((tile.moisture + tile.temperature) / 2, densityPower), minPerTile, maxPerTile) * densityMulti);
	}

	public override Entity[] Render(Tile tile, Entity parent)
	{
		var count = GetDecorEntityCount(tile);
		var entities = new Entity[count];
		for (int i = 0; i < count; i++)
		{
			var size = Random.Range(minSize, maxSize);
			var height = Random.Range(minHeight, maxHeight);
			var pos = new Vector3(Random.value, 0, Random.value) * (tile.Coords.innerRadius - (size/2f));
			entities[i] = meshEntity.Instantiate(pos + tile.SurfacePoint, new Vector3(size, height, size));
		}
		return entities;

	}
}
