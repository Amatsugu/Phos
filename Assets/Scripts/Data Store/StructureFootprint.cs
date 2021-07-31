using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;

[Serializable]
public struct StructureFootprint
{
	[Range(0,3)]
	public int size;
	public int2[] footprint;

	public HashSet<HexCoords> GetFootpint(HexCoords center, int rotation = 0)
	{
		var fp = new HashSet<HexCoords>();
		for (int i = 0; i < footprint.Length; i++)
			fp.Add(new HexCoords(center.X + footprint[i].x, center.Y + footprint[i].y, center.edgeLength).RotateAround(center, rotation));
		return fp;
	}

	public HexCoords[] GetOccupiedTiles(HexCoords center, int rotation = 0)
	{
		var fp = new HexCoords[footprint.Length];
		for (int i = 0; i < footprint.Length; i++)
			fp[i] = new HexCoords(center.X + footprint[i].x, center.Y + footprint[i].y, center.edgeLength).RotateAround(center, rotation);
		return fp;
	}

	public List<HexCoords> GetNeighbors(HexCoords center, int rotation = 0)
	{
		var footprint = GetOccupiedTiles(center, rotation);
		var occupiedSet = new HashSet<HexCoords>(footprint);

		var result = new List<HexCoords>(footprint.Length * 2);

		for (int i = 0; i < footprint.Length; i++)
		{
			var curTile = footprint[i];
			for (int d = 0; d < 6; d++)
			{
				var n = curTile.GetNeighbor(d);
				if (occupiedSet.Contains(n))
					continue;
				result.Add(n);
			}
		}
		return result;
	}
}
