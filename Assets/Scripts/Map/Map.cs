using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class Map : IDisposable
{
	public static Map ActiveMap;
	public static EntityManager EM;
	public bool IsRendered { get; private set; }

	public string Name;
	public readonly int height;
	public readonly int totalHeight;
	public readonly int width;
	public readonly int totalWidth;
	public readonly int length;
	public readonly float tileEdgeLength;
	public readonly float tongDiagonal;
	public readonly float shortDiagonal;
	public readonly float innerRadius;

	public float seaLevel;
	public int Seed { get; private set; }

	public Transform Parent { get; }
	public Chunk[] Chunks { get; }

	public HQTile HQ;
	public Dictionary<int, MobileUnit> units;
	private int _nextId = 0;

	public Map(int height, int width, int seed, float edgeLength = 1)
	{
		this.width = width;
		this.height = height;
		totalWidth = width * Chunk.SIZE;
		totalHeight = height * Chunk.SIZE;
		Seed = seed;
		UnityEngine.Random.InitState(seed);
		length = width * height;
		Chunks = new Chunk[length];
		tileEdgeLength = edgeLength;
		innerRadius = Mathf.Sqrt(3f) / 2f * tileEdgeLength;
		shortDiagonal = Mathf.Sqrt(3f) * tileEdgeLength;
		tongDiagonal = 2 * tileEdgeLength;
		units = new Dictionary<int, MobileUnit>(500);
		ActiveMap = this;
	}

	public struct Chunk
	{
		public const int SIZE = 32;
		public Tile[] Tiles;
		public HexCoords chunkCoord;
		public bool isShown;
		public bool isCreated;

		private Bounds _bounds;
		private NativeArray<Entity> _chunkTiles;


		public Chunk(HexCoords coord)
		{
			isShown = false;
			isCreated = true;
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
			n.SetBiome(tile.biomeId, tile.moisture, tile.temperature);
			this[chunkCoord] = n;
			_chunkTiles[chunkCoord.ToIndex(SIZE)] = n.Render();
			if (n is BuildingTile b)
				b.originalTile = tile.info;
			return n;
		}
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
			var index = chunkX + chunkZ * width;
			if (index < 0 || index >= length)
				return null;
			var chunk = Chunks[index];
			if (!chunk.isCreated)
				return null;
			return chunk[coord.ToChunkLocalCoord(chunkX, chunkZ)];
		}

		set
		{
			var (chunkX, chunkZ) = coord.GetChunkPos();
			var chunk = Chunks[chunkX + chunkZ * width];
			if (!chunk.isCreated)
				chunk = new Chunk(HexCoords.FromOffsetCoords(chunkX, chunkZ, tileEdgeLength));
			chunk[coord.ToChunkLocalCoord(chunkX, chunkZ)] = value;
			Chunks[chunkX + chunkZ * width] = chunk;
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
			return this[new HexCoords(x, y, tileEdgeLength)];
		}

	}

	public MobileUnit AddUnit(MobileUnitInfo unitInfo, Tile tile)
	{
		var id = _nextId++;
		var unit = new MobileUnit(id, unitInfo, tile);
		units.Add(id, unit);
		if (IsRendered)
			unit.Render();
		return unit;
	}

	public TileInfo[] GetTileTyes()
	{
		return Chunks.SelectMany(c => c.Tiles.Select(t => t.info)).Distinct().ToArray();
	}

	internal void UpdateView(Plane[] camPlanes)
	{
		var chunksChanged = 0;

		for (int i = 0; i < Chunks.Length; i++)
		{
			if (Chunks[i].Show(Chunks[i].InView(camPlanes)))
				chunksChanged++;
		}
	}

	public void Render(EntityManager entityManager)
	{
		if (IsRendered)
			return;
		IsRendered = true;
		if (EM == null)
			EM = entityManager;
		foreach (var unit in units)
		{
			if (!unit.Value.IsRendered)
				unit.Value.Render();
		}
		var start = DateTime.Now;
		for (int i = 0; i < Chunks.Length; i++)
			Chunks[i].Render();
		Debug.Log($"Render... {(DateTime.Now - start).TotalMilliseconds}ms");
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
			increment = innerRadius;
		for (float i = 0; i < distance; i += increment)
		{
			var p = ray.GetPoint(i);
			var t = this[HexCoords.FromPosition(p)];
			if (t == null)
				continue;
			if (p.y > t.Height + tileEdgeLength)
				continue;
			if(p.y <= t.Height && p.y >= 0)
			{
				var a = t.Coords.worldXZ;
				var b = p;
				b.y = 0;
				if ((a - b).sqrMagnitude <= tileEdgeLength * tileEdgeLength)
					return t;
			}
			if ((t.SurfacePoint - p).sqrMagnitude <= tileEdgeLength * tileEdgeLength)
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
		radius *= innerRadius;
		if (radius == 0)
		{
			selection.Add(this[center]);
			return selection;
		}
		for (float x = -radius; x < radius; x++)
		{
			for (float z = -radius; z < radius; z++)
			{
				var p = HexCoords.FromPosition(new Vector3(x + center.worldX, 0, z + center.worldZ), innerRadius);
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
		innerRadius *= tongDiagonal;
		outerRadius *= tongDiagonal;
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
			d -= innerRadius * innerRadius * tongDiagonal;
			d = MathUtils.Map(d, 0, (outerRadius * outerRadius * tongDiagonal) - (innerRadius * innerRadius * tongDiagonal), 0, 1);
			tile.UpdateHeight(Mathf.Lerp(tile.Height, height, 1 - d));
		}
	}

	public Tile ReplaceTile(Tile tile, TileInfo newTile)
	{
		if (!IsRendered)
			throw new Exception("Cannot use ReplaceTile for an unrendered map");
		var coord = tile.Coords;
		var (chunkX, chunkZ) = coord.GetChunkPos();
		var index = chunkX + chunkZ * width;
		var localCoord = coord.ToChunkLocalCoord(chunkX, chunkZ);
		var nT = Chunks[index].ReplaceTile(localCoord, newTile);
		if (nT is HQTile t)
			HQ = t;
	
		tile.OnRemoved();
		tile.Destroy();
		nT.OnPlaced();
		return nT;
	}

	public void RevertTile(BuildingTile tile)
	{
		if (tile.originalTile != null)
			ReplaceTile(tile, tile.originalTile);
		else
			Debug.LogWarning("No Original Tile to revert to");
	}

	public Tile[] GetNeighbors(Tile tile) => GetNeighbors(tile.Coords);

	public List<Tile> GetPath(Tile src, Tile dst, float maxIncline = float.MaxValue, Func<Tile, bool?> filter = null)
	{
		if (src == dst)
			return null;
		PathNode BestFScore(HashSet<PathNode> nodes)
		{
			PathNode best = nodes.First();
			foreach (var node in nodes)
			{
				if (best.CalculateF(dst) > node.CalculateF(dst))
					best = node;
			}
			return best;
		}

		var open = new HashSet<PathNode>
		{
			new PathNode(src, 1)
		};
		var closed = new HashSet<PathNode>();
		var dstNode = new PathNode(dst, 1);
		PathNode last = null;
		while (open.Count > 0)
		{
			if (closed.Contains(dstNode))
				break;
			PathNode curTileNode;
			curTileNode = BestFScore(open);
			open.Remove(curTileNode);
			closed.Add(curTileNode);
			last = curTileNode;
			foreach (Tile neighbor in GetNeighbors(curTileNode.tile))
			{
				if (neighbor == null)
					continue;
				if (Mathf.Abs(neighbor.Height - curTileNode.tile.Height) > maxIncline)
					continue;
				if(filter != null && filter(neighbor) != true && !neighbor.Equals(dst))
				{
					continue;
				}
				var adj = new PathNode(neighbor, curTileNode.G + 1, curTileNode);
				if (closed.Contains(adj))
					continue;
				if (!open.Contains(adj))
					open.Add(adj);
				else
				{
					var o = open.First(oAdj => oAdj.Equals(adj));
					if (adj.CalculateF(dst) < o.CalculateF(dst))
					{
						open.Remove(o);
						open.Add(adj);
					}
				}
			}
			if (open.Count > 1000)
			{
				Debug.LogWarning("Big Path");
				return null;
			}
		}
		if (open.Count == 0)
			return null;
		var curNode = last;
		if (curNode == null)
			return null;
		List<Tile> path = new List<Tile>();
		do
		{
			path.Add(curNode.tile);
			curNode = curNode.src;
		} while (curNode != null);
		path.Reverse();
		return path;
	}

	private class PathNode
	{
		public Tile tile;
		public int G;
		public PathNode src;

		public PathNode(Tile tile, int g, PathNode src = null)
		{
			this.tile = tile;
			G = g;
			this.src = src;
		}


		public float CalculateF(global::Tile b)
		{
			var d = tile.SurfacePoint - b.SurfacePoint;
			return G + (Mathf.Abs(d.x) + Mathf.Abs(d.y) + Mathf.Abs(d.z));
		}

		public override bool Equals(object obj)
		{
			if (obj is PathNode n)
			{
				return n.tile.Equals(tile);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return tile.GetHashCode();
		}
	}


	public float GetHeight(Vector3 worldPos, int radius = 0) => GetHeight(HexCoords.FromPosition(worldPos, tileEdgeLength), radius);

	public float GetHeight(HexCoords coord, int radius = 0)
	{
		if (radius == 0)
		{
			var t = this[coord];
			if (t == null)
				return this.seaLevel;
			return t.Height;
		}
		var selection = this.HexSelect(coord, radius);
		if (selection.Count == 0)
			return seaLevel;
		var max = selection.Max(t => t.Height);
		return (max < seaLevel) ? seaLevel : max;
	}

	public void Destroy()
	{
		if (!IsRendered)
			return;
		foreach (var chunk in Chunks)
			chunk.Destroy();
		IsRendered = false;
	}



	#region IDisposable Support
	private bool disposedValue = false; // To detect redundant calls

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
			}

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
