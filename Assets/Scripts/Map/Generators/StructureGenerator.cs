using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class StructureGenerator : FeatureGenerator
{
    public int2 count;
    public BuildingTileInfo tile;
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
                break;
            attempts++;
            var coord = HexCoords.FromOffsetCoords(rand.NextInt(0, map.totalWidth), rand.NextInt(0, map.totalHeight), map.tileEdgeLength);
            var tile = map[coord];


            if (coordsToGen.Any(c => c == coord))
                break;
            if (coordsToGen.Any(c => c.isCreated && c.Distance(coord) <= minDist))
                break;
            if (tile.Height <= altitudeRange.x || tile.Height >= altitudeRange.y)
                break;



            coordsToGen[curCount++] = coord;
            attempts = 0;
        }

        for (int i = 0; i < countToGen; i++)
        {
            if (!coordsToGen[i].isCreated)
                break;
            map.ReplaceTile(map[coordsToGen[i]], tile);
        }

    }
}
