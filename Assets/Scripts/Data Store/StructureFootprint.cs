using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;

[Serializable]
public struct StructureFootprint
{
	public int size;
	public int2[] footprint;

	public HashSet<HexCoords> GetFootpint(HexCoords center)
	{
		var fp = new HashSet<HexCoords>();
		for (int i = 0; i < footprint.Length; i++)
			fp.Add(new HexCoords(center.X + footprint[i].x, center.Y + footprint[i].y, center.edgeLength));
		return fp;
	}
}
