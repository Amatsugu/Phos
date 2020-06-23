using Amatsugu.Phos.Tiles;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile Decorators/Trees")]
public class TreeDecorator : TileDecorator
{
	public int minPerTile = 0;
	public int maxPerTile = 3;
	public float densityPower = 1;
	public float minHeight = .5f;
	public float maxHeight = 4;
	public float minSize = 0.1f;
	public float maxSize = .5f;
	public NoiseSettings noise;
	public float noiseScale = 250;

	private INoiseFilter _filter;
	private System.Random _rand;

	private void OnDisable()
	{
		_filter = null;
	}

	public override int GetDecorEntityCount(Tile tile)
	{
		if (_filter == null)
		{
			_filter = NoiseFilterFactory.CreateNoiseFilter(this.noise, tile.map.Seed);
			_rand = new System.Random(tile.map.Seed);
		}
		var noise = Mathf.Pow(Mathf.PerlinNoise(tile.Coords.OffsetCoords.x / noiseScale, tile.Coords.OffsetCoords.y / noiseScale), densityPower);
		noise = MathUtils.Remap(Mathf.Clamp(noise, 0, 1), 0, 1, minPerTile, maxPerTile);
		return Mathf.RoundToInt(noise * densityMulti);
	}

	public override void Render(Tile tile, NativeSlice<Entity> decor)
	{
		var count = GetDecorEntityCount(tile);
		for (int i = 0; i < count; i++)
		{
			var size = _rand.Range(minSize, maxSize);
			var height = _rand.Range(minHeight, maxHeight);
			var pos = new float3(_rand.NextFloat(), 0, _rand.NextFloat()) * (tile.map.innerRadius);
			decor[i] = meshEntity.Instantiate(pos + tile.SurfacePoint, new Vector3(size, height, size), Quaternion.Euler(0, _rand.Range(0, 360), 0));
		}
	}
}