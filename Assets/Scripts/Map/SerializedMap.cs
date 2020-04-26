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

	public Map Deserialize()
	{
		Debug.LogWarning("Map deserializtion not implemented");
		var datbaseHandle = Addressables.LoadAssetAsync<TileDatabase>("Tile Database");
		var db = datbaseHandle.Task.GetAwaiter().GetResult();
		return LoadMap(db);
	}

	private Map LoadMap(TileDatabase db)
	{
		var map = new Map(height, width, seed, tileEdgeLength, true);
		for (int i = 0; i < tiles.Length; i++)
		{
			var curTile = tiles[i];
			map[curTile.pos] = db.tileEntites[curTile.tileId].tile.CreateTile(curTile.pos, curTile.height);
			map[curTile.pos].OnDeSerialized(curTile.tileData);
		}
		return map;
	}
}

//[Serializable]
public struct SeializedTile
{
	public int tileId;
	public float height;
	public HexCoords pos;
	public Dictionary<string, string> tileData;
}