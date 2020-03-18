using System;
using System.IO;

using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Generator/Random")]
public class RandomGenerator : MapGenerator
{
	[System.Serializable]
	public struct NoiseLayer
	{
		public string name;
		public bool enabled;
		public bool useFirstLayerAsMask;
		public NoiseSettings noiseSettings;
	}

	public int borderSize = 16;
	public AnimationCurve borderCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	public BiomePainter biomePainter;

	public float noiseScale = .5f;
	public NoiseLayer[] noiseLayers;
	public bool useSeed;
#if DEBUG
	public bool useSeedDev;
#endif
	public int seed = 11;
	public float landSeaRatio = .4f;

	[HideInInspector]
	public bool biomeFold;

	[HideInInspector]
	public bool preview;

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
		var startTime = DateTime.Now;
#if DEBUG
		if (!(useSeed || useSeedDev))
			seed = startTime.GetHashCode();
#else
		if (!useSeed)
			seed = startTime.GetHashCode();
#endif
		InitFilters();
		Map map = new Map((int)Size.x, (int)Size.y, seed, edgeLength)
		{
			seaLevel = seaLevel
		};
		var heightMap = new float[map.totalWidth * map.totalHeight];
		var landToSeaRatio = 0f;
		float min = float.MaxValue, max = float.MinValue;
		for (int z = 0; z < map.totalWidth; z++)
		{
			for (int x = 0; x < map.totalHeight; x++)
			{
				var sample = GenerateHeight(x, z);
				if (sample > seaLevel)
					landToSeaRatio++;
				if (sample < min)
					min = sample;
				if (sample > max)
					max = sample;
				heightMap[x + z * map.totalWidth] = sample;
			}
		}
		landToSeaRatio /= heightMap.Length;
		//Prevent the ratio of land to sea from being too low
		if (landToSeaRatio <= landSeaRatio)
		{
			reject++;
			if (reject < 10)
				goto Start;
			else
				UnityEngine.Debug.LogWarning("Unalble to find satisfactory level");
		}
		UnityEngine.Debug.Log($"Generate HightMap... {(DateTime.Now - startTime).TotalMilliseconds}ms {reject} Rejects");
		startTime = DateTime.Now;
		var tempMap = biomePainter.GetTempMap(map.totalWidth, map.totalHeight, heightMap, min, max, seaLevel);
		UnityEngine.Debug.Log($"Generate Temp map... {(DateTime.Now - startTime).TotalMilliseconds}ms");
		startTime = DateTime.Now;
		var moistureMap = biomePainter.GetMoistureMap(map.totalWidth, map.totalHeight, noiseFilters[0], noiseScale);
		UnityEngine.Debug.Log($"Generate Mouseture map... {(DateTime.Now - startTime).TotalMilliseconds}ms");
		//SaveBiomeMaps(tempMap, moistureMap, map.totalWidth, map.totalHeight);
		startTime = DateTime.Now;
		for (int z = 0; z < map.totalWidth; z++)
		{
			for (int x = 0; x < map.totalHeight; x++)
			{
				var coord = HexCoords.FromOffsetCoords(x, z, edgeLength);
				var i = x + z * map.totalWidth;
				var height = heightMap[i];
				var (tInfo, biomeId) = biomePainter.GetTile(moistureMap[i], tempMap[i], height, seaLevel);
				map[coord] = tInfo.CreateTile(coord, height).SetBiome(biomeId, moistureMap[i], tempMap[i]);
			}
		}
		UnityEngine.Debug.Log($"Paint map... {(DateTime.Now - startTime).TotalMilliseconds}ms");
		UnityEngine.Debug.Log($"Done... {(DateTime.Now - totalStartTime).TotalMilliseconds}ms");
		return map;
	}

	public void InitFilters()
	{
		noiseFilters = new INoiseFilter[noiseLayers.Length];
		for (int i = 0; i < noiseLayers.Length; i++)
			noiseFilters[i] = NoiseFilterFactory.CreateNoiseFilter(noiseLayers[i].noiseSettings, seed);
	}

	public void SaveBiomeMaps(float[] tMap, float[] mMap, int h, int w)
	{
		var tTex = new Texture2D(w, h);
		var mTex = new Texture2D(w, h);
		var bTex = new Texture2D(w, h);
		var colors = new Color[16];
		for (int i = 0; i < 16; i++)
		{
			colors[i] = Color.HSVToRGB(MathUtils.Remap(i, 0, 16, 0, 1), .5f, .5f);
		}
		for (int z = 0; z < h; z++)
		{
			for (int x = 0; x < w; x++)
			{
				var i = x + z * w;
				tTex.SetPixel(x, z, colors[Mathf.RoundToInt(tMap[i])]);
				mTex.SetPixel(x, z, colors[15 - Mathf.RoundToInt(mMap[i])]);
				bTex.SetPixel(x, z, colors[Mathf.RoundToInt(tMap[i]) + Mathf.RoundToInt(mMap[i]) * 4]);
			}
		}
		File.WriteAllBytes("tMap.png", tTex.EncodeToPNG());
		File.WriteAllBytes("mMap.png", mTex.EncodeToPNG());
		File.WriteAllBytes("bMap.png", bTex.EncodeToPNG());
	}

	public float GenerateHeight(float x, float z)
	{
		float elevation = 0;
		float firstLayer = 0;
		var point = new Vector3(x, 0, z) / noiseScale;
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
		var borderT = 1f;
		var w = (int)Size.x * Map.Chunk.SIZE;
		var h = (int)Size.y * Map.Chunk.SIZE;
		if (x <= borderSize)
		{
			borderT *= MathUtils.Remap(x, 0, borderSize, 0, 1);
		}
		if (x >= w - borderSize)
		{
			borderT *= 1 - MathUtils.Remap(x, w - borderSize, w, 0, 1);
		}
		if (z <= borderSize)
		{
			borderT *= MathUtils.Remap(z, 0, borderSize, 0, 1);
		}
		if (z >= h - borderSize)
		{
			borderT *= 1 - MathUtils.Remap(z, h - borderSize, h, 0, 1);
		}
		borderT = Mathf.Max(borderT, 0);
		return Mathf.Max(0.2f, Mathf.Lerp(0.2f, elevation, borderCurve.Evaluate(borderT)));
	}
}