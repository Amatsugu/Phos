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
			_filter = NoiseFilterFactory.CreateNoiseFilter(this.noise, Map.ActiveMap.Seed);
			_rand = new System.Random(Map.ActiveMap.Seed);
		}
		var noise = Mathf.Pow(Mathf.PerlinNoise(tile.Coords.offsetX / noiseScale, tile.Coords.offsetZ / noiseScale), densityPower);
		noise = MathUtils.Remap(Mathf.Clamp(noise, 0, 1), 0, 1, minPerTile, maxPerTile);
		return Mathf.RoundToInt(noise * densityMulti);
	}

	public override Entity[] Render(Tile tile)
	{
		var count = GetDecorEntityCount(tile);
		var entities = new Entity[count];
		for (int i = 0; i < count; i++)
		{
			var size = _rand.Range(minSize, maxSize);
			var height = _rand.Range(minHeight, maxHeight);
			var pos = new Vector3(_rand.NextFloat(), 0, _rand.NextFloat()) * (Map.ActiveMap.innerRadius);
			entities[i] = meshEntity.Instantiate(pos + tile.SurfacePoint, new Vector3(size, height, size), Quaternion.Euler(0, _rand.Range(0, 360), 0));
		}
		return entities;

	}

	public override Matrix4x4[] GetTransformMatricies(Tile tile)
	{
		var transforms = new Matrix4x4[GetDecorEntityCount(tile)];
		for (int i = 0; i < transforms.Length; i++)
		{
			var size = _rand.Range(minSize, maxSize);
			var height = _rand.Range(minHeight, maxHeight);
			var pos = new Vector3(_rand.NextFloat(), 0, _rand.NextFloat()) * (Map.ActiveMap.innerRadius);
			transforms[i] = Matrix4x4.TRS(tile.SurfacePoint + pos, Quaternion.Euler(0, _rand.Range(0, 360), 0), new Vector3(size, height, size));
		}
		return transforms;
	}
}
