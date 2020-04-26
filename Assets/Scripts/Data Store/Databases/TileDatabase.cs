using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Tile Database")]
public class TileDatabase : ScriptableObject, ISerializationCallbackReceiver
{
	[SerializeField]
	[HideInInspector]
	public Dictionary<int, TileDefination> tileEntites;

	[SerializeField]
	private int[] tileIds;
	[SerializeField]
	private TileDefination[] tileDefs;

	public void OnAfterDeserialize()
	{
		tileEntites = new Dictionary<int, TileDefination>();
		for (int i = 0; i < tileIds.Length; i++)
			tileEntites.Add(tileIds[i], tileDefs[i]);
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
		public int Id => tile.GetInstanceID();
		public TileType type;
		public TileEntity tile;
	}

	public TileDefination RegisterTile(TileEntity tile)
	{
		if (tileEntites.ContainsKey(tile.GetInstanceID()))
			throw new ArgumentException("Tile is already in database");
		var tileDef = new TileDefination
		{
			tile = tile,
			type = TileType.Tile
		};
		if (tile is BuildingTileEntity)
			tileDef.type |= TileType.Building;
		if (tile is ResourceTileInfo)
			tileDef.type |= TileType.Resource;

		tileEntites.Add(tileDef.Id, tileDef);

		return tileDef;
	}
}
