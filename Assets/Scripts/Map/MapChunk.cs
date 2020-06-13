using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;

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
	private NativeArray<Entity> _chunkTiles;

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
		_chunkTiles = new NativeArray<Entity>(SIZE * SIZE, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
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
		var color = inView ? Color.red : Color.blue;
		UnityEngine.Debug.DrawLine(new Vector3(_bounds.min.x, 0, _bounds.min.z), new Vector3(_bounds.min.x, 0, _bounds.max.z), color);
		UnityEngine.Debug.DrawLine(new Vector3(_bounds.min.x, 0, _bounds.max.z), new Vector3(_bounds.max.x, 0, _bounds.max.z), color);
		UnityEngine.Debug.DrawLine(new Vector3(_bounds.max.x, 0, _bounds.max.z), new Vector3(_bounds.max.x, 0, _bounds.min.z), color);
		UnityEngine.Debug.DrawLine(new Vector3(_bounds.min.x, 0, _bounds.min.z), new Vector3(_bounds.max.x, 0, _bounds.min.z), color);
		UnityEngine.Debug.DrawLine(new Vector3(_bounds.min.x, 0, _bounds.min.z), new Vector3(_bounds.min.x, SIZE, _bounds.min.z), color);
		return inView;
	}

	public void Destroy()
	{
		try
		{
			if (!isRendered)
			{
				foreach (var tile in _chunkTiles)
					Map.EM.DestroyEntity(tile);
			}
		}
		catch
		{
		}
		finally
		{
			if (_chunkTiles.IsCreated)
				_chunkTiles.Dispose();
		}
		for (int i = 0; i < Tiles.Length; i++)
			Tiles[i].Destroy();
	}

	public bool Show(bool shown)
	{
		if (!isRendered && shown)
		{
			Render();
			return true;
		}
		if (!isRendered)
			return false;
		if (shown == isShown)
			return false;
		for (int i = 0; i < Tiles.Length; i++)
			Tiles[i].Show(shown);
		if (shown)
			Map.EM.RemoveComponent(_chunkTiles, typeof(FrozenRenderSceneTag));
		else
			Map.EM.AddComponent(_chunkTiles, typeof(FrozenRenderSceneTag));
		isShown = shown;
		return true;
	}

	internal void Render()
	{
		isShown = true;
		isRendered = true;
		if (!_chunkTiles.IsCreated)
			_chunkTiles = new NativeArray<Entity>(SIZE * SIZE, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		var renderBounds = new AABB
		{
			Center = Bounds.center,
			Extents = Bounds.extents
		};
		for (int i = 0; i < SIZE * SIZE; i++)
		{
			_chunkTiles[i] = Tiles[i].Render();
			Map.EM.SetComponentData(_chunkTiles[i], new ChunkWorldRenderBounds
			{
				Value = renderBounds
			});
			Tiles[i].RenderDecorators();
		}
	}

	internal void RenderDecorators()
	{
		for (int i = 0; i < Tiles.Length; i++)
		{
			Tiles[i].RenderDecorators();
		}
	}

	public Tile ReplaceTile(HexCoords chunkCoord, TileEntity newTile)
	{
		if (!isRendered)
			Render();
		var tile = this[chunkCoord];
		var n = newTile.CreateTile(map, tile.Coords, tile.Height);
		n.SetBiome(tile.biomeId, tile.moisture, tile.temperature);
		if (tile.originalTile == null)
			n.originalTile = tile.info;
		else
			n.originalTile = tile.originalTile;
		this[chunkCoord] = n;
		_chunkTiles[chunkCoord.ToIndex(SIZE)] = n.Render();
		return n;
	}
}
