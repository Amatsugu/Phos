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
		var map = new Map(height, width, seed, tileEdgeLength, false);
		Debug.LogWarning("Map deserializtion not implemented");
		return map;
	}

	private void LoadTile(Map map, int i = 0)
	{
		if (i >= tiles.Length)
			return;
		var curTile = tiles[i];
		/*Addressables.LoadAssetsAsync<TileEntity>(tiles.Select(t => t.assetReference).Distinct().ToArray(), te =>
		{
			Debug.Log($"<color=red>[!]</color>Loaded {te.name}");
		}, Addressables.MergeMode.None);*/
		/*
		var tileEntity = new AssetReference(curTile.assetReference).LoadAssetAsync<TileEntity>();
		tileEntity.Completed += e =>
		{
			//LoadTile(map, i++);
			if (e.Status == AsyncOperationStatus.Succeeded)
			{
				map[curTile.pos] = e.Result.CreateTile(curTile.pos, curTile.height);
				map[curTile.pos].OnDeSerialized(curTile.tileData);
			}
			else
				Debug.LogWarning($"Failed to load tile: [{curTile.pos}] ({curTile.assetReference})");
		};
		*/
	}
}

//[Serializable]
public struct SeializedTile
{
	public string assetReference;
	public float height;
	public HexCoords pos;
	public Dictionary<string, string> tileData;
}