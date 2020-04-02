using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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
		var ops = new AsyncOperationHandle<TileEntity>[tiles.Length];
		for (int i = 0; i < tiles.Length; i++)
		{
			var curTile = tiles[i];
			var tileEntity = curTile.assetReference.LoadAssetAsync<TileEntity>();
			ops[i] = tileEntity;
			tileEntity.Completed += e =>
			{
				if (e.Status == AsyncOperationStatus.Succeeded)
				{
					map[curTile.pos] = e.Result.CreateTile(curTile.pos, curTile.height);
					map[curTile.pos].OnDeSerialized(curTile.tileData);
				}
				else
					Debug.LogWarning($"Failed to load tile: [{curTile.pos}] ({curTile.assetReference})");
			};
		}
		/*bool isDone = false;
		while(!isDone)
		{
			isDone = true;
			for (int i = 0; i < ops.Length; i++)
			{
				if (!ops[i].IsDone)
				{
					isDone = false;
					break;
				}
			}
		}*/
		return map;
	}
}

public struct SeializedTile
{
	public AssetReference assetReference;
	public float height;
	public HexCoords pos;
	public Dictionary<string, string> tileData;
}