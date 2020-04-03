using System.Linq;

using Unity.Mathematics;

using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Generator/Feature/Structures")]
public class StructureGenerator : FeatureGenerator
{
	public int2 count;
	public BuildingTileEntity tile;
	public float minDist;
	public float2 altitudeRange;

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
			var origTile = map[coordsToGen[i]];
			map[coordsToGen[i]] = tile.CreateTile(coordsToGen[i], origTile.Height);
			//if(tile.preserveGroundTile)
			map[coordsToGen[i]].originalTile = origTile.info;
		}
	}
}