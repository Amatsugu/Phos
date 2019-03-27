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
		if (!useSeed)
			seed = (int)(new System.DateTime(1990, 1, 1) - System.DateTime.Now).TotalSeconds;
		Random.InitState(seed);
		maxElevation = float.MinValue;
		noiseFilters = new INoiseFilter[noiseLayers.Length];
		for (int i = 0; i < noiseLayers.Length; i++)
			noiseFilters[i] = NoiseFilterFactory.CreateNoiseFilter(noiseLayers[i].noiseSettings, seed);
		Map map = new Map((int)Size.x, (int)Size.y, parent, edgeLength)
		{
			SeaLevel = seaLevel
		};
		var chunkSize = Map.Chunk.SIZE;
		for (int z = 0; z < map.Width * chunkSize; z++)
		{
			for (int x = 0; x < map.Height * chunkSize; x++)
			{
				var coord = HexCoords.FromOffsetCoords(x, z, edgeLength);
				var height = GenerateHeightMap(x, z);
				var tInfo = tileMapper.GetTile(height, seaLevel);
				map[coord] = tInfo.CreateTile(coord, height);
			}
		}
		return map;
	}

	public float GenerateHeightMap(int x, int y)
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
