using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

//[Serializable]
public class SerializedMap
{
	public int height, width;
	public int seed;
	public float tileEdgeLength;
	public float seaLevel;
	public string name;
	public SeializedTile[] tiles;

	public Map Deserialize(TileDatabase db)
	{
		var map = new Map(height, width, seed, tileEdgeLength, false);
		for (int i = 0; i < tiles.Length; i++)
		{
			var curTile = tiles[i];
			map[curTile.pos] = db.tileEntites[curTile.tileId].tile.CreateTile(map, curTile.pos, curTile.height);
			if (curTile.origTile != -1)
				map[curTile.pos].originalTile = db.tileEntites[curTile.origTile].tile;
			map[curTile.pos].OnDeSerialized(curTile.tileData);
		}
		return map;
	}
}

//[Serializable]
public struct SeializedTile
{
	public int tileId;
	public int origTile;
	public float height;
	public HexCoords pos;
	public Dictionary<string, string> tileData;
}