using System;
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

	private float maxElevation = float.MinValue;

	public override Tile PaintTile(Tile tile)
	{
		tile.info = tileMapper.GetTile(tile.Height, seaLevel, maxElevation);
		return tile;
	}

	public override Map GenerateMap(Transform parent = null)
	{
		var reject = 0;
		var totalStartTime = DateTime.Now;
		Start:
		if (!useSeed)
			seed = (int)(new System.DateTime(1990, 1, 1) - System.DateTime.Now).TotalSeconds;
		UnityEngine.Random.InitState(seed);
		maxElevation = float.MinValue;
		var startTime = DateTime.Now;
		noiseFilters = new INoiseFilter[noiseLayers.Length];
		for (int i = 0; i < noiseLayers.Length; i++)
			noiseFilters[i] = NoiseFilterFactory.CreateNoiseFilter(noiseLayers[i].noiseSettings, seed);
		Map map = new Map((int)Size.x, (int)Size.y, edgeLength)
		{
			SeaLevel = seaLevel
		};
		var chunkSize = Map.Chunk.SIZE;
		var heightMap = new float[map.Width * map.Height * chunkSize * chunkSize];
		var landToSeaRatio = 0f;
		for (int z = 0; z < map.Width * chunkSize; z++)
		{
			for (int x = 0; x < map.Height * chunkSize; x++)
			{
				var sample = GenerateHeight(x, z);
				if (sample > seaLevel)
					landToSeaRatio++;
				heightMap[x + z * (map.Width * chunkSize)] = sample;
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
		var tempMap = biomePainter.GetTempMap(map.Width * chunkSize, map.Height * chunkSize, heightMap, seaLevel, seed);
		Debug.Log($"Generate Temp map... {(DateTime.Now - startTime).TotalMilliseconds}ms");
		startTime = DateTime.Now;
		var moustureMap = biomePainter.GetMoistureMap(map.Width * chunkSize, map.Height * chunkSize, noiseFilters[0], noiseScale, seaLevel);
		Debug.Log($"Generate Mouseture map... {(DateTime.Now - startTime).TotalMilliseconds}ms");
		startTime = DateTime.Now;
		for (int z = 0; z < map.Width * chunkSize; z++)
		{
			for (int x = 0; x < map.Height * chunkSize; x++)
			{
				var coord = HexCoords.FromOffsetCoords(x, z, edgeLength);
				var i = x + z * (map.Width * chunkSize);
				var height = heightMap[i];
                //var tInfo = tileMapper.GetTile(height, seaLevel);
                int biomeId = Mathf.RoundToInt(tempMap[i]) + Mathf.RoundToInt(moustureMap[i]) * 4;
                var tInfo = biomePainter.GetTile(biomeId, height, seaLevel);
				map[coord] = tInfo.CreateTile(coord, height).SetBiome(biomeId, moustureMap[i], tempMap[i]);
			}
		}
		Debug.Log($"Paint map... {(DateTime.Now - startTime).TotalMilliseconds}ms");
		Debug.Log($"Done... {(DateTime.Now - startTime).TotalMilliseconds}ms");
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
