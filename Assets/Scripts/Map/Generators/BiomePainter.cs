using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile Mapper/Biome Painter")]
public class BiomePainter : ScriptableObject
{
	public TileInfo[] biomes;

	public float noiseScale;
	public NoiseSettings settings;


	public int[] GetBiomeMap(int width, int height, int seed)
	{
		var heightMap = new float[width * height];
		float min = float.MaxValue, max = float.MinValue;
		var n = NoiseFilterFactory.CreateNoiseFilter(settings, seed);
		for (int z = 0; z < height; z++)
		{
			for (int x = 0; x < width; x++)
			{
				var sample = n.Evaluate(new Vector3(x / noiseScale, 0, z / noiseScale));
				if (min > sample)
					min = sample;
				if (max < sample)
					max = sample;
				heightMap[x + z * width] = sample;
			}
		}

		var biomeMap = new int[width * height];
		for (int i = 0; i < biomeMap.Length; i++)
		{
			biomeMap[i] = Mathf.Clamp(Mathf.RoundToInt(MathUtils.Map(heightMap[i], min, max, 0, biomes.Length-1)), 0, biomes.Length-1);
		}

		return biomeMap;
	}

	public int[] GetMoistureMap(int width, int height, INoiseFilter filter, float noiseScale, float seaLevel)
	{
		var heightMap = new float[width * height];
		float min = seaLevel, max = float.MinValue;
		for (int z = 0; z < height; z++)
		{
			for (int x = 0; x < width; x++)
			{
				var sample = filter.Evaluate(new Vector3(x / noiseScale, 0, z / noiseScale));
				//if (min > sample)
					//min = sample;
				if (max < sample)
					max = sample;
				heightMap[x + z * width] = sample;
			}
		}

		var moistureMap = new int[width * height];
		for (int i = 0; i < moistureMap.Length; i++)
		{
			moistureMap[i] = Mathf.Clamp(Mathf.RoundToInt(MathUtils.Map(heightMap[i], min, max, 0, 3)), 0, 3);
		}

		return moistureMap;
	}

	public int[] GetTempMap(int width, int height, float[] heightMap, float seaLevel, int seed)
	{
		Random.InitState(seed);
		var tempMap = new int[width * height];
		var equator = Random.Range(0, height);
		var maxD = Mathf.Abs((height / 2) - equator);
		maxD = maxD < equator ? equator : maxD;
		var maxHeight = heightMap.Max() - seaLevel;
		for (int z = 0; z < height; z++)
		{
			for (int x = 0; x < width; x++)
			{
				var d = Mathf.Abs(z - equator);
				var tE = MathUtils.Map(d, 0, maxD, 0, 3);
				var tH = MathUtils.Map(heightMap[x + z * width], 0, maxHeight, 0, 3);
				var t = Mathf.RoundToInt((tH + tE) / 2f);
				tempMap[x + z * width] = t;
			}
		}
		return tempMap;
	}


	public TileInfo GetTile(int biome, float height, float seaLevel)
	{
		//if (height <= seaLevel)
			//return biomes[0];
		return biomes[biome];
	}

}
