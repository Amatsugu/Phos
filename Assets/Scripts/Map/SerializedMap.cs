using Amatsugu.Phos.DataStore;
using Amatsugu.Phos.Tiles;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;

using TMPro;

using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;


//[Serializable]
public class SerializedMap
{
	public int height, width;
	public int seed;
	public float tileEdgeLength;
	public float seaLevel;
	public string name;
	public SerializedTile[] tiles;
	public SerializedUnit[] units;
	public SerializedConduitGrapth conduitGrapth;

	public Map Deserialize(TileDatabase tileDb, UnitDatabase unitDb)
	{
		var map = new Map(height, width, seed, tileEdgeLength)
		{
			conduitGraph = conduitGrapth.Deserialize(),
			seaLevel = seaLevel
		};
		for (int i = 0; i < tiles.Length; i++)
		{
			var curTile = tiles[i];
			var pos = new HexCoords(curTile.x, curTile.y, tileEdgeLength);
			map[pos] = tileDb.tileEntites[curTile.tileId].tile.CreateTile(map, pos, curTile.height);
			if (curTile.origTile != -1)
				map[pos].originalTile = tileDb.tileEntites[curTile.origTile].tile;
			map[pos].OnDeSerialized(curTile.tileData);
		}
		map.units = new Dictionary<int, MobileUnit>();
		for (int i = 0; i < units.Length; i++)
		{
			var sUnit = units[i];
			map.AddUnit(unitDb.unitEntites[sUnit.unitId].unit, map[new HexCoords(sUnit.x, sUnit.y, tileEdgeLength)], sUnit.faction);
		}
		return map;
	}
}

public struct SerializedTile
{
	public int tileId;
	public int origTile;
	public float height;
	public int x, y;
	public Dictionary<string, string> tileData;
}

public struct SerializedUnit
{
	public int unitId;
	public int x, y;
	public Faction faction;
}