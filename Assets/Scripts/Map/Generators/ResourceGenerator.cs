using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Generator/Feature/Resource")]
public class ResourceGenerator : FeatureGenerator
{
	public ResourceTileInfo resource;
	[Range(0f, 1f)]
	public float density;

	public NoiseSettings settings;
	public float noiseScale;

	[HideInInspector]
	public bool preview;
	[HideInInspector]
	public Texture2D previewTex;

	private INoiseFilter _filter;
	private float _adjustedDensity;

	private void OnDisable()
	{
		_filter = null;
	}

	public override void Generate(Map map)
	{
		if (_filter == null)
		{
			_filter = NoiseFilterFactory.CreateNoiseFilter(settings, map.Seed);
			_adjustedDensity = MathUtils.Map(density, 0, 1, 0, 1 - (settings.type == NoiseSettings.FilterType.Simple ? settings.simpleNoiseSettings.minValue : settings.rigidNoiseSettings.minValue));
		}
		for (int z = 0; z < map.totalHeight; z++)
		{
			for (int x = 0; x < map.totalWidth; x++)
			{
				var pos = HexCoords.FromOffsetCoords(x, z, map.tileEdgeLength);
				var h = map[pos].Height;
				if (h < Map.ActiveMap.seaLevel)
					continue;
				if (GetSample(x,z) == 1)
				{
					var res = resource.CreateTile(pos, h);
					res.originalTile = map[pos].info;
					map[pos] = res;
				}
			}
		}
	}

	public float GetSample(int x, int z, INoiseFilter filter = null)
	{
		if (filter == null)
			filter = _filter;
		else
			_adjustedDensity = MathUtils.Map(density, 0, 1, 0, 1 - (settings.type == NoiseSettings.FilterType.Simple ? settings.simpleNoiseSettings.minValue : settings.rigidNoiseSettings.minValue));
		var sample = 1 - filter.Evaluate(new Vector3(x / noiseScale, z / noiseScale));
		if (sample <= _adjustedDensity)
		{
			return 1;
		}
		return 0;
	}
}
