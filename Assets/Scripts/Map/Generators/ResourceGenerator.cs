using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Generator/Feature/Resource")]
public class ResourceGenerator : FeatureGenerator
{
	public ResourceTileInfo resource;

	[Range(1, 10)]
	public int ratity = 3;

	[Range(0f, 1f)]
	public float density = .5f;

	[HideInInspector]
	public bool preview;

	public int[] PrepareMap(int width, int height, int seed = 0)
	{
		var rand = new System.Random(seed + name.GetHashCode());
		var resourceMap = new int[width * height];
		for (int i = 0; i < resourceMap.Length; i++)
		{
			resourceMap[i] = rand.NextDouble() >= density ? 1 : 0;
		}
		for (int i = 0; i < ratity; i++)
		{
			DistributeResource(resourceMap, width, height);
		}
		return resourceMap;
	}

	private void DistributeResource(int[] map, int w, int h)
	{
		int GetNeighborCount(int gX, int gZ)
		{
			var c = 0;
			for (int z = gZ - 1; z <= gZ + 1; z++)
			{
				for (int x = gX - 1; x <= gX + 1; x++)
				{
					if (x < 0 || z < 0 || x >= w || z >= h)
					{
						c++;
						continue;
					}
					if (x != gX || z != gZ)
						c += map[x + z * w];
				}
			}
			return c;
		}
		for (int z = 0; z < h; z++)
		{
			for (int x = 0; x < w; x++)
			{
				var nt = GetNeighborCount(x, z);
				if (nt > 4)
					map[x + z * w] = 1;
				else if (nt < 4)
					map[x + z * w] = 0;
			}
		}
	}

	public override void Generate(Map map)
	{
		var resourceMap = PrepareMap(map.totalWidth, map.totalHeight, map.Seed);
		for (int z = 0; z < map.totalHeight; z++)
		{
			for (int x = 0; x < map.totalWidth; x++)
			{
				var pos = HexCoords.FromOffsetCoords(x, z, map.tileEdgeLength);
				if (map[pos] is ResourceTile)
					continue;
				var h = map[pos].Height;
				if (h < map.seaLevel)
					continue;
				if (resourceMap[x + z * map.totalWidth] == 0)
				{
					var res = resource.CreateTile(map, pos, h);
					res.originalTile = map[pos].info;
					map[pos] = res;
				}
			}
		}
	}
}