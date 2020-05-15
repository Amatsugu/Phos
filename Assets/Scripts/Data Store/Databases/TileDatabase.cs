using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Tile Database")]
public class TileDatabase : ScriptableObject, ISerializationCallbackReceiver
{
	[HideInInspector]
	public Dictionary<int, TileDefination> tileEntites;
	[HideInInspector]
	public Dictionary<TileEntity, int> entityIds;

	[SerializeField]
	private int[] tileIds;
	[SerializeField]
	private TileDefination[] tileDefs;
	private int nextId = 0;

	public void OnAfterDeserialize()
	{
		tileEntites = new Dictionary<int, TileDefination>();
		entityIds = new Dictionary<TileEntity, int>();
		for (int i = 0; i < tileIds.Length; i++)
		{
			tileEntites.Add(tileIds[i], tileDefs[i]);
			entityIds.Add(tileDefs[i].tile, tileIds[i]);
			if(tileIds[i] > nextId)
				nextId = tileIds[i];
		}
		nextId++;
	}

	public void Reset()
	{
		if (Application.isPlaying)
			return;
		nextId = 0;
		tileEntites.Clear();
		entityIds.Clear();
		tileDefs = null;
		tileIds = null;
	}

	public void OnBeforeSerialize()
	{
		if (tileEntites == null)
			return;
		tileIds = tileEntites.Keys.ToArray();
		tileDefs = tileEntites.Values.ToArray();
	}

	public enum TileType
	{
		Tile,
		Resource,
		Building
	}

	[Serializable]
	public struct TileDefination
	{
		public int id;
		public TileType type;
		public TileEntity tile;
	}

	public bool RegisterTile(TileEntity tile, out TileDefination tileDef)
	{
		if (entityIds.ContainsKey(tile))
		{
			tileDef = default;
			return false;
		}
		tileDef = new TileDefination
		{
			id = nextId++,
			tile = tile,
			type = TileType.Tile
		};
		if (tile is BuildingTileEntity)
			tileDef.type |= TileType.Building;
		if (tile is ResourceTileInfo)
			tileDef.type |= TileType.Resource;

		tileEntites.Add(tileDef.id, tileDef);
		entityIds.Add(tile, tileDef.id);
		return true;
	}
}
