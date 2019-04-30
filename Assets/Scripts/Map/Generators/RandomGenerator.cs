﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


[CreateAssetMenu(menuName = "Map Asset/Generator/Random")]
public class RandomGenerator : MapGenerator
{

	[System.Serializable]
	public struct NoiseLayer
	{
		public bool enabled;
		public bool useFirstLayerAsMask;
		public NoiseSettings noiseSettings;
	}

	public int borderSize = 16;
	public AnimationCurve borderCurve = AnimationCurve.EaseInOut(0,0,1,1);
	public BiomePainter biomePainter;
	[HideInInspector]
	public bool biomeFold;
	public float noiseScale = .5f;
	

	public NoiseLayer[] noiseLayers;
	public bool useSeed;
	public int seed = 11;

	private INoiseFilter[] noiseFilters;

	private void OnEnable()
	{
		noiseFilters = null;
	}

	public override Map GenerateMap(Transform parent = null)
	{
		var reject = 0;
		var totalStartTime = DateTime.Now;
		Start:
		if (!useSeed)
			seed = (int)(new System.DateTime(1990, 1, 1) - System.DateTime.Now).TotalSeconds;
		UnityEngine.Random.InitState(seed);
		var startTime = DateTime.Now;
		noiseFilters = new INoiseFilter[noiseLayers.Length];
		for (int i = 0; i < noiseLayers.Length; i++)
			noiseFilters[i] = NoiseFilterFactory.CreateNoiseFilter(noiseLayers[i].noiseSettings, seed);
		Map map = new Map((int)Size.x, (int)Size.y, seed, edgeLength)
		{
			seaLevel = seaLevel
		};
		var heightMap = new float[map.totalWidth * map.totalHeight];
		var landToSeaRatio = 0f;
		for (int z = 0; z < map.totalWidth; z++)
		{
			for (int x = 0; x < map.totalHeight; x++)
			{
				var sample = GenerateHeight(x, z);
				if (sample > seaLevel)
					landToSeaRatio++;
				var borderT = 1f;
				if(x <= borderSize) 
				{
					borderT *= MathUtils.Map(x, 0, borderSize, 0, 1);
				}
				if(x >= map.totalWidth - borderSize)
				{
					borderT *= 1-MathUtils.Map(x, map.totalWidth - borderSize, map.totalWidth, 0, 1);
				}
				if (z <= borderSize)
				{
					borderT *= MathUtils.Map(z, 0, borderSize, 0, 1);
				}
				if (z >= map.totalHeight - borderSize)
				{
					borderT *= 1-MathUtils.Map(z, map.totalHeight - borderSize, map.totalHeight, 0, 1);
				}
				borderT = Mathf.Max(borderT, 0);
				heightMap[x + z * map.totalWidth] = Mathf.Lerp(0.2f, sample, borderCurve.Evaluate(borderT));
			}
		}
		landToSeaRatio /= heightMap.Length;
		//Prevent the ratio of land to sea from being too low
		if (landToSeaRatio < .4f)
		{
			reject++;
			goto Start;
		}
		Debug.Log($"Generate HightMap... {(DateTime.Now-startTime).TotalMilliseconds}ms {reject} Rejects");
		startTime = DateTime.Now;
		var tempMap = biomePainter.GetTempMap(map.totalWidth, map.totalHeight, heightMap, seaLevel, seed);
		Debug.Log($"Generate Temp map... {(DateTime.Now - startTime).TotalMilliseconds}ms");
		startTime = DateTime.Now;
		var moustureMap = biomePainter.GetMoistureMap(map.totalWidth, map.totalHeight, noiseFilters[0], noiseScale, seaLevel);
		Debug.Log($"Generate Mouseture map... {(DateTime.Now - startTime).TotalMilliseconds}ms");
		startTime = DateTime.Now;
		for (int z = 0; z < map.totalWidth; z++)
		{
			for (int x = 0; x < map.totalHeight; x++)
			{
				var coord = HexCoords.FromOffsetCoords(x, z, edgeLength);
				var i = x + z * map.totalWidth;
				var height = heightMap[i];
                int biomeId = Mathf.RoundToInt(tempMap[i]) + Mathf.RoundToInt(moustureMap[i]) * 4;
                var tInfo = biomePainter.GetTile(biomeId, height, seaLevel);
				map[coord] = tInfo.CreateTile(coord, height).SetBiome(biomeId, moustureMap[i], tempMap[i]);
			}
		}
		Debug.Log($"Paint map... {(DateTime.Now - startTime).TotalMilliseconds}ms");
		Debug.Log($"Done... {(DateTime.Now - totalStartTime).TotalMilliseconds}ms");
		return map;
	}

	public float GenerateHeight(int x, int y)
	{
		float elevation = 0;
		float firstLayer = 0;
		var point = new Vector3(x, 0, y) / noiseScale;
		if (noiseFilters.Length > 0)
		{
			firstLayer = noiseFilters[0].Evaluate(point);
			if (noiseLayers[0].enabled)
				elevation = firstLayer;
		}
		for (int i = 1; i < noiseFilters.Length; i++)
		{
			if (noiseLayers[i].enabled)
			{
				float mask = (noiseLayers[i].useFirstLayerAsMask) ? firstLayer - seaLevel : 1;
				mask = Mathf.Max(0, mask);
				elevation += noiseFilters[i].Evaluate(point) * mask;
			}
		}
		return elevation;
	}
}
