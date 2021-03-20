using Amatsugu.Phos;
using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;

using System;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public struct MapChunk
{
	public const int SIZE = 32;
	public Tile[] Tiles;
	public bool isShown;
	public bool isCreated;
	public bool isRendered;
	public readonly Map map;

	internal Bounds Bounds => _bounds;

	private Bounds _bounds;

	public MapChunk(Map map, int offsetX, int offsetZ, float tileEdgeLength, float shortDiagonal)
	{
		isShown = false;
		isRendered = false;
		isCreated = true;
		this.map = map;
		Tiles = new Tile[SIZE * SIZE];
		var worldCoord = HexCoords.FromOffsetCoords(offsetX * SIZE, offsetZ * SIZE, tileEdgeLength);
		_bounds = new Bounds
		{
			min = worldCoord.WorldPos,
			max = worldCoord.WorldPos + new float3(SIZE * shortDiagonal, 100, SIZE * 1.5f)
		};
	}

	public Tile this[HexCoords coord]
	{
		get
		{
			return Tiles[coord.ToIndex(SIZE)];
		}

		set => Tiles[coord.ToIndex(SIZE)] = value;
	}

	public bool InView(Plane[] camPlanes)
	{
		var inView = GeometryUtility.TestPlanesAABB(camPlanes, _bounds);
#if UNITY_EDITOR
		var color = inView ? Color.red : Color.blue;
		Debug.DrawLine(new Vector3(_bounds.min.x, 0, _bounds.min.z), new Vector3(_bounds.min.x, 0, _bounds.max.z), color);
		Debug.DrawLine(new Vector3(_bounds.min.x, 0, _bounds.max.z), new Vector3(_bounds.max.x, 0, _bounds.max.z), color);
		Debug.DrawLine(new Vector3(_bounds.max.x, 0, _bounds.max.z), new Vector3(_bounds.max.x, 0, _bounds.min.z), color);
		Debug.DrawLine(new Vector3(_bounds.min.x, 0, _bounds.min.z), new Vector3(_bounds.max.x, 0, _bounds.min.z), color);
		Debug.DrawLine(new Vector3(_bounds.min.x, 0, _bounds.min.z), new Vector3(_bounds.min.x, SIZE, _bounds.min.z), color);
#endif
		return inView;
	}

	[Obsolete]
	public T ReplaceTile<T>(HexCoords chunkCoord, T newTile) where T : Tile
	{
		var tile = this[chunkCoord];
		newTile.SetBiome(tile.biomeId, tile.moisture, tile.temperature);
		if (tile.originalTile == null)
			newTile.originalTile = tile.info;
		else
			newTile.originalTile = tile.originalTile;
		this[chunkCoord] = newTile;
		return newTile;
	}

}
