using Amatsugu.Phos;
using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;

using System.Linq;

using Unity.Mathematics;

using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Generator/Feature/Structures")]
public class StructureGenerator : FeatureGenerator
{
	[Header("Core")]
	public int2 count;
	public BuildingTileEntity tile;
	public float minDist;
	public float2 altitudeRange;
	[Header("Crystal")]
	public ResourceTileInfo phosCrystal;
	public int crystalRange = 4;
	[Range(0,100)]
	public int density = 50;

	public override void Generate(Map map)
	{
		var rand = new Unity.Mathematics.Random((uint)map.Seed);
		var countToGen = rand.NextInt(count.x, count.y);

		var coordsToGen = new HexCoords[countToGen];
		int curCount = 0;

		int attempts = 0;

		//Select location
		while (curCount < countToGen)
		{
			if (attempts > 1024)
			{
				UnityEngine.Debug.Log("Abort, too many Attempts");
				break;
			}
			attempts++;
			var coord = HexCoords.FromOffsetCoords(rand.NextInt(0, map.totalWidth), rand.NextInt(0, map.totalHeight), map.tileEdgeLength);
			var tile = map[coord];

			if (coordsToGen.Any(c => c.isCreated && c.Equals(coord)))
				continue;
			if (coordsToGen.Any(c => c.isCreated && c.Distance(coord) <= minDist))
				continue;
			if (tile.Height <= altitudeRange.x || tile.Height >= altitudeRange.y)
				continue;

			coordsToGen[curCount++] = coord;
			attempts = 0;
		}

		UnityEngine.Debug.Log($"Genetating {curCount} cores");
		for (int i = 0; i < curCount; i++)
		{
			if (!coordsToGen[i].isCreated)
				break;
			PlaceCore(map, coordsToGen[i], ref rand);
		}
	}

	public void PlaceCore(Map map, HexCoords coords, ref Unity.Mathematics.Random rand)
	{
		var origTile = map[coords];
		map[coords] = tile.CreateTile(map, coords, origTile.Height);
		map[coords].originalTile = origTile.GetGroundTileInfo();
		var ring = HexCoords.SpiralSelect(coords, crystalRange, true);
		for (int i = 0; i < ring.Length; i++)
		{
			if (rand.NextInt(100) <= density)
				PlaceCrystal(map, ring[i]);
		}
	}

	public void PlaceCrystal(Map map, HexCoords coords)
	{
		var origTile = map[coords];
		if (origTile == null)
			return;
		map[coords] = phosCrystal.CreateTile(map, coords, origTile.Height);
		map[coords].originalTile = origTile.GetGroundTileInfo();
	}
}