using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Generator/Feature/Resource")]
public class ResourceGenerator : FeatureGenerator
{
	public ResourceTileInfo resource;
	[Range(0f, 1f)]
	public float rarity;
	[Range(0f, 1f)]
	public float density;

	public NoiseSettings settings;
	public float noiseScale;

	public override void Generate(Map map)
	{
		//TODO: Write generator for resources
		var filter = NoiseFilterFactory.CreateNoiseFilter(settings, map.Seed);
		var adjustedDensity = MathUtils.Map(density, 0, 1, settings.type == NoiseSettings.FilterType.Simple ? settings.simpleNoiseSettings.strength : settings.rigidNoiseSettings.strength, 1);

		for (int z = 0; z < map.totalHeight; z++)
		{
			for (int x = 0; x < map.totalWidth; x++)
			{
				var sample = 1-filter.Evaluate(new Vector3(x / noiseScale, z / noiseScale));
				if (sample <= adjustedDensity)
				{
					var pos = HexCoords.FromOffsetCoords(x, z, map.tileEdgeLength);
					var h = map[pos].Height;
					map[pos] = resource.CreateTile(pos, h);
				}
			}
		}
	}
}
