using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class Map : IDisposable
{
	public static Map ActiveMap;
	public static EntityManager EM;

	public int Height { get; }
	public int Width { get; }
	public int Length => Chunks.Length;
	public float TileEdgeLength { get; }
	public float LongDiagonal { get; }
	public float ShortDiagonal { get; }

	public float InnerRadius { get; }
	public float SeaLevel;
	public Transform Parent { get; }
	public Chunk[] Chunks { get; }

	public HQTile HQ;
	private List<BuildingTile> _powerTransferTiles;

	public class Chunk
	{
		public const int SIZE = 16;
		public Tile[] Tiles;
		public HexCoords chunkCoord;
		public bool isShown = false;

		private Bounds _bounds;
		private NativeArray<Entity> _chunkTiles;
		public Chunk(HexCoords coord)
		{
			chunkCoord = coord;
			Tiles = new Tile[SIZE * SIZE];
			var worldCoord = HexCoords.FromOffsetCoords(coord.offsetX * SIZE, coord.offsetZ * SIZE, coord.edgeLength);
			_bounds = new Bounds
			{
				min = worldCoord.worldXZ,
				max = worldCoord.worldXZ + new Vector3(SIZE * coord.shortDiagonal, 100, SIZE * 1.5f)
			};
			_chunkTiles = new NativeArray<Entity>(SIZE * SIZE, Allocator.Persistent);
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
			Debug.DrawLine(new Vector3(_bounds.min.x, 0, _bounds.min.z), new Vector3(_bounds.min.x, 0, _bounds.max.z), color);
			Debug.DrawLine(new Vector3(_bounds.min.x, 0, _bounds.max.z), new Vector3(_bounds.max.x, 0, _bounds.max.z), color);
			Debug.DrawLine(new Vector3(_bounds.max.x, 0, _bounds.max.z), new Vector3(_bounds.max.x, 0, _bounds.min.z), color);
			Debug.DrawLine(new Vector3(_bounds.min.x, 0, _bounds.min.z), new Vector3(_bounds.max.x, 0, _bounds.min.z), color);
			Debug.DrawLine(new Vector3(_bounds.min.x, 0, _bounds.min.z), new Vector3(_bounds.min.x, SIZE, _bounds.min.z), color);
			return inView;
		}

		public void Destroy()
		{
			try
			{
				foreach (var tile in _chunkTiles)
					EM.DestroyEntity(tile);
			}
			catch
			{

			}
			finally
			{
				if (_chunkTiles.IsCreated)
					_chunkTiles.Dispose();
				_chunkTiles = default;
			}
			for (int i = 0; i < Tiles.Length; i++)
				Tiles[i].Destroy();
		}

		public bool Show(bool shown)
		{
			if (shown == isShown)
				return false;
			for (int i = 0; i < Tiles.Length; i++)
				Tiles[i].Show(shown);
			if (shown)
			{
				EM.RemoveComponent(_chunkTiles, typeof(Frozen));
			}
			else
			{
				EM.AddComponent(_chunkTiles, typeof(Frozen));

			}
			isShown = shown;
			return true;
		}

		internal void Render()
		{
			isShown = true;
			if(!_chunkTiles.IsCreated)
				_chunkTiles = new NativeArray<Entity>(SIZE * SIZE, Allocator.Persistent);
			for (int i = 0; i < SIZE * SIZE; i++)
			{
				_chunkTiles[i] = Tiles[i].Render();
			}
		}

		public Tile ReplaceTile(HexCoords chunkCoord, TileInfo newTile)
		{
			var tile = this[chunkCoord];
			var n = newTile.CreateTile(tile.Coords, tile.Height);
			this[chunkCoord] = n;
			tile.OnRemoved();
			tile.Destroy();
			_chunkTiles[chunkCoord.ToIndex(SIZE)] = n.Render();
			n.OnPlaced();
			return n;
		}
	}

	public Map(int height, int width, float edgeLength = 1)
	{
		Width = width;
		Height = height;
		Chunks = new Chunk[width * height];
		TileEdgeLength = edgeLength;
		InnerRadius = Mathf.Sqrt(3f) / 2f * TileEdgeLength;
		ShortDiagonal = Mathf.Sqrt(3f) * TileEdgeLength;
		LongDiagonal = 2 * TileEdgeLength;
		_powerTransferTiles = new List<BuildingTile>();
		ActiveMap = this;
	}

	/// <summary>
	/// Get a tile the given HexCoords position
	/// </summary>
	/// <param name="coord">Position</param>
	/// <returns>Tile at position</returns>
	public Tile this[HexCoords coord]
	{
		get
		{
			var (chunkX, chunkZ) = coord.GetChunkPos();
			var index = chunkX + chunkZ * Width;
			if (index < 0 || index >= Length)
				return null;
			var chunk = Chunks[index];
			if (chunk == null)
				return null;
			return chunk[coord.ToChunkLocalCoord(chunkX, chunkZ)];
		}

		set
		{
			var (chunkX, chunkZ) = coord.GetChunkPos();
			var chunk = Chunks[chunkX + chunkZ * Width];
			if (chunk == null)
				chunk = Chunks[chunkX + chunkZ * Width] = new Chunk(HexCoords.FromOffsetCoords(chunkX, chunkZ, TileEdgeLength));
			chunk[coord.ToChunkLocalCoord(chunkX, chunkZ)] = value;
		}
	}


	/// <summary>
	/// Get a tile at the given HexCoords Position
	/// </summary>
	/// <param name="x">X component</param>
	/// <param name="y">Y component</param>
	/// <param name="z">Z component</param>
	/// <returns>Tile at position</returns>
	public Tile this[int x, int y, int z]
	{
		get
		{
			return this[new HexCoords(x, y, TileEdgeLength)];
		}

	}

	public TileInfo[] GetTileTyes()
	{
		return Chunks.SelectMany(c => c.Tiles.Select(t => t.info)).Distinct().ToArray();
	}

	internal void UpdateView(Plane[] camPlanes)
	{
		var chunksChanged = 0;
		foreach (var chunk in Chunks)
		{
			if (chunk == null)
				continue;
			if (chunk.Show(chunk.InView(camPlanes)))
				chunksChanged++;
		}
	}

	public void Render(EntityManager entityManager)
	{
		if (EM == null)
			EM = entityManager;
		foreach (var chunk in Chunks)
			chunk?.Render();
	}

	/// <summary>
	/// Get Neihboring tiles
	/// </summary>
	/// <param name="coords">Center tile location</param>
	/// <returns>Neihboring tiles arranged clockwise starting from the left</returns>
	public Tile[] GetNeighbors(HexCoords coords)
	{
		Tile[] neighbors = new Tile[6];
		neighbors[0] = this[coords.x - 1, coords.y, coords.z + 1]; //Left
		neighbors[1] = this[coords.x - 1, coords.y + 1, coords.z]; //Top Left
		neighbors[2] = this[coords.x, coords.y + 1, coords.z - 1]; //Top Right
		neighbors[3] = this[coords.x + 1, coords.y, coords.z - 1]; //Right
		neighbors[4] = this[coords.x + 1, coords.y - 1, coords.z]; //Bottom Right
		neighbors[5] = this[coords.x, coords.y - 1, coords.z + 1]; //Bottom Left
		return neighbors;
	}

	/// <summary>
	/// Raycasting to the surface of a tile, accurate within the OuterRadius of a tile
	/// </summary>
	/// <param name="ray">Ray to cast</param>
	/// <returns>The tile if found</returns>
	public Tile GetTileFromRay(Ray ray, float distance = 50000f, float increment = 0.1f)
	{
		if (increment == 0.1f)
			increment = InnerRadius;
		for (float i = 0; i < distance; i += increment)
		{
			var p = ray.GetPoint(i);
			var t = this[HexCoords.FromPosition(p)];
			if (t == null)
				continue;
			if (p.y > t.Height + TileEdgeLength)
				continue;
			if(p.y <= t.Height && p.y >= 0)
			{
				var a = t.Coords.worldXZ;
				var b = p;
				b.y = 0;
				if ((a - b).sqrMagnitude <= TileEdgeLength * TileEdgeLength)
					return t;
			}
			if ((t.SurfacePoint - p).sqrMagnitude <= TileEdgeLength * TileEdgeLength)
				return t;
		}
		return null;
	}

	public List<Tile> HexSelect(HexCoords center, int radius)
	{
		var selection = new List<Tile>();
		radius = Mathf.Abs(radius);
		if (radius == 0)
		{
			selection.Add(this[center]);
			return selection;
		}
		for (int y = -radius; y <= radius; y++)
		{
			int xMin = -radius, xMax = radius;
			if (y < 0)
				xMin = -radius - y;
			if (y > 0)
				xMax = radius - y;
			for (int x = xMin; x <= xMax; x++)
			{
				int z = -x - y;
				var t = this[center.x + x, center.y + y, center.z + z];
				if (t != null)
					selection.Add(t);
			}
		}
		return selection;
	}

	public List<Tile> CircularSelect(HexCoords center, float radius)
	{
		var selection = new List<Tile>();
		radius = Mathf.Abs(radius);
		radius *= InnerRadius;
		if (radius == 0)
		{
			selection.Add(this[center]);
			return selection;
		}
		for (float x = -radius; x < radius; x++)
		{
			for (float z = -radius; z < radius; z++)
			{
				var p = HexCoords.FromPosition(new Vector3(x + center.worldX, 0, z + center.worldZ), InnerRadius);
				var d = Mathf.Pow(p.worldX - center.worldX, 2) + Mathf.Pow(p.worldZ - center.worldZ, 2);
				if (d <= radius * radius)
				{
					var t = this[p];
					if (t != null)
						selection.Add(t);
				}
			}
		}
		return selection;
	}

	public enum FlattenMode
	{
		Center,
		Average
	}

	public void CircularFlatten(HexCoords center, float innerRadius, float outerRadius, FlattenMode mode = FlattenMode.Center)
	{
		innerRadius *= LongDiagonal;
		outerRadius *= LongDiagonal;
		var innerSelection = CircularSelect(center, innerRadius);
		var c = this[center];
		float height = c.Height;
		if (mode == FlattenMode.Average)
			height = innerSelection.Average(t => t.Height);

		foreach (var tile in innerSelection)
		{
			tile.UpdateHeight(height);
		}

		if (outerRadius <= innerRadius)
			return;
		var outerSelection = CircularSelect(center, outerRadius).Except(innerSelection);
		foreach (var tile in outerSelection)
		{
			var d = Mathf.Pow(center.worldX - tile.Coords.worldX, 2) + Mathf.Pow(center.worldZ - tile.Coords.worldZ, 2);
			d -= innerRadius * innerRadius;
			d = MathUtils.Map(d, 0, (outerRadius * outerRadius) - (innerRadius * innerRadius), 0, 1);
			tile.UpdateHeight(Mathf.Lerp(tile.Height, height, 1 - d));
		}
	}

	public void HexFlatten(HexCoords center, int innerRadius, int outerRadius, FlattenMode mode = FlattenMode.Center)
	{
		if (innerRadius == 0 || outerRadius == 0)
			return;
		var innerSelection = HexSelect(center, innerRadius);
		var c = this[center];
		float height = c.Height;
		if (mode == FlattenMode.Average)
			height = innerSelection.Average(t => t.Height);

		foreach (var tile in innerSelection)
		{
			tile.UpdateHeight(height);
		}

		if (outerRadius <= innerRadius)
			return;
		var outerSelection = HexSelect(center, outerRadius).Except(innerSelection);
		foreach (var tile in outerSelection)
		{
			var d = Mathf.Pow(center.worldX - tile.Coords.worldX, 2) + Mathf.Pow(center.worldZ - tile.Coords.worldZ, 2);
			d -= innerRadius * innerRadius * LongDiagonal;
			d = MathUtils.Map(d, 0, (outerRadius * outerRadius * LongDiagonal) - (innerRadius * innerRadius * LongDiagonal), 0, 1);
			tile.UpdateHeight(Mathf.Lerp(tile.Height, height, 1 - d));
		}
	}

	public List<Tile> GetPoweredTiles()
	{
		return _powerTransferTiles.SelectMany(t => HexSelect(t.Coords, t.buildingInfo.powerTransferRadius)).Distinct().ToList();
	}

	public Tile ReplaceTile(Tile tile, TileInfo newTile)
	{
		var coord = tile.Coords;
		var (chunkX, chunkZ) = coord.GetChunkPos();
		var index = chunkX + chunkZ * Width;
		var localCoord = coord.ToChunkLocalCoord(chunkX, chunkZ);
		var chunk = Chunks[index];
		var nT = chunk.ReplaceTile(localCoord, newTile);
		switch(nT)
		{
			case HQTile t:
				_powerTransferTiles.Add(HQ = t);
				break;
			case PoweredBuildingTile t:
				_powerTransferTiles.Add(t);
				break;
		}
		return nT;
	}

	public Tile[] GetNeighbors(Tile tile) => GetNeighbors(tile.Coords);

	public void Destroy()
	{
		foreach (var chunk in Chunks)
			chunk?.Destroy();
	}



	#region IDisposable Support
	private bool disposedValue = false; // To detect redundant calls

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				// TODO: dispose managed state (managed objects).
			}

			// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
			// TODO: set large fields to null.
			Destroy();

			disposedValue = true;
		}
	}

	// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
	~Map()
	{
		// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		Dispose(false);
	}

	// This code added to correctly implement the disposable pattern.
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		Dispose(true);
		// TODO: uncomment the following line if the finalizer is overridden above.
		GC.SuppressFinalize(this);
	}
	#endregion
}
