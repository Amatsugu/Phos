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

	public HashSet<HexCoords> GetFootpint(HexCoords center)
	{
		var fp = new HashSet<HexCoords>();
		for (int i = 0; i < footprint.Length; i++)
			fp.Add(new HexCoords(center.X + footprint[i].x, center.Y + footprint[i].y, center.edgeLength));
		return fp;
	}

	public HexCoords[] GetOccupiedTiles(HexCoords center)
	{
		var fp = new HexCoords[footprint.Length];
		for (int i = 0; i < footprint.Length; i++)
			fp[i] = (new HexCoords(center.X + footprint[i].x, center.Y + footprint[i].y, center.edgeLength));
		return fp;
	}
}
