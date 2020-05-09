using System.Linq;

using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Generator/Feature/Geothermal Vents")]
public class VentGenerator : FeatureGenerator
{
	public int rarity;
	public VentTileInfo tileInfo;

	public override void Generate(Map map)
	{
		int vents = 0;
		System.Random rand = new System.Random(map.Seed + name.GetHashCode());
		for (int z = 0; z < map.totalHeight; z++)
		{
			for (int x = 0; x < map.totalWidth; x++)
			{
				if (rand.Next(0, rarity) != 0)
					continue;
				var center = map[HexCoords.FromOffsetCoords(x, z, map.tileEdgeLength)];
				if (!center.IsUnderwater)
					continue;
				var coord = center.Coords;
				if (center is GeothermalVentTile || center is GeothermalVentShellTile)
					continue;
				var n = map.GetNeighbors(coord);
				if (n.Any(t => t == null || t is GeothermalVentTile || t is GeothermalVentShellTile))
					continue;
				var h = center.Height;
				map[coord] = tileInfo.CreateTile(map, coord, h);
				map[coord].originalTile = center.info;
				//map.CircularFlatten(coord, 1, 3);
				for (int i = 0; i < n.Length; i++)
				{
					var orig = n[i].info;
					map[n[i].Coords] = new GeothermalVentShellTile(n[i].Coords, h, i * 60, map, tileInfo);
					map[n[i].Coords].originalTile = orig;
				}
				vents++;
			}
		}
		UnityEngine.Debug.Log($"{GeneratorName}: {vents} vents generated");
	}
}