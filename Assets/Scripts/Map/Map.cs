using DataStore.ConduitGraph;
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
	public readonly float longDiagonal;
	public readonly float shortDiagonal;
	public readonly float innerRadius;

	public float seaLevel;
	public int Seed { get; private set; }

	public Chunk[] Chunks { get; }

	public HQTile HQ;
	public ConduitGraph conduitGraph;
	public Dictionary<int, MobileUnit> units;
	public List<int>[] unitLocations;
	private int _nextId = 1;

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
		longDiagonal = 2 * tileEdgeLength;
		units = new Dictionary<int, MobileUnit>(500);
		unitLocations = new List<int>[height * width];
		ActiveMap = this;
	}

	public struct Chunk
	{
		public const int SIZE = 32;
		public Tile[] Tiles;
		public bool isShown;
		public bool isCreated;
		public bool isRendered;

		internal Bounds Bounds => _bounds;

		private Bounds _bounds;
		private NativeArray<Entity> _chunkTiles;


		public Chunk(int offsetX, int offsetZ)
		{
			isShown = false;
			isRendered = false;
			isCreated = true;
			Tiles = new Tile[SIZE * SIZE];
			var worldCoord = HexCoords.FromOffsetCoords(offsetX * SIZE, offsetZ * SIZE, ActiveMap.tileEdgeLength);
			_bounds = new Bounds
			{
				min = worldCoord.worldXZ,
				max = worldCoord.worldXZ + new Vector3(SIZE * ActiveMap.shortDiagonal, 100, SIZE * 1.5f)
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
				if (!isRendered)
				{
					foreach (var tile in _chunkTiles)
						EM.DestroyEntity(tile);
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
			if(!isRendered && shown)
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
				EM.RemoveComponent(_chunkTiles, typeof(FrozenRenderSceneTag));
			else
				EM.AddComponent(_chunkTiles, typeof(FrozenRenderSceneTag));
			isShown = shown;
			return true;
		}

		internal void Render()
		{
			isShown = true;
			isRendered = true;
			if(!_chunkTiles.IsCreated)
				_chunkTiles = new NativeArray<Entity>(SIZE * SIZE, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < SIZE * SIZE; i++)
			{
				_chunkTiles[i] = Tiles[i].Render();
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

		public Tile ReplaceTile(HexCoords chunkCoord, TileInfo newTile)
		{
			if (!isRendered)
				Render();
			var tile = this[chunkCoord];
			var n = newTile.CreateTile(tile.Coords, tile.Height);
			n.SetBiome(tile.biomeId, tile.moisture, tile.temperature);
			n.originalTile = tile.info;
			this[chunkCoord] = n;
			_chunkTiles[chunkCoord.ToIndex(SIZE)] = n.Render();
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
				chunk = new Chunk(chunkX, chunkZ);
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
	public Tile this[int x, int y]
	{
		get
		{
			return this[new HexCoords(x, y, tileEdgeLength, innerRadius)];
		}

	}

	public MobileUnit AddUnit(MobileUnitInfo unitInfo, Tile tile)
	{
		var id = _nextId++;
		var chunkIndex = tile.Coords.GetChunkIndex(width);
		if(unitLocations[chunkIndex] == null)
			unitLocations[chunkIndex] = new List<int>();
		var unit = new MobileUnit(id, unitInfo, tile, chunkIndex);
		unitLocations[chunkIndex].Add(id);
		units.Add(id, unit);
		if (IsRendered)
			unit.Render();
		return unit;
	}

	public void MoveUnit(int unitId, int srcChunk, int dstChunk)
	{
		if (srcChunk == dstChunk)
			return;
		if (unitLocations[srcChunk].Remove(unitId))
		{
			if (unitLocations[dstChunk] == null)
				unitLocations[dstChunk] = new List<int>();
			unitLocations[dstChunk].Add(unitId);
		}
	}

	public void UpdateUnitChunks()
	{
		foreach (var unit in units)
		{
			unit.Value.UpdateChunk();
		}
	}

	public List<int> SelectUnits(HexCoords left, HexCoords right)
	{
		//Transform to chunk space
		var cLeft = left.GetChunkPos();
		var cRight = right.GetChunkPos();
		int cZMin, cZMax;
		int cXMin, cXMax;
		Vector3 worldLeft, worldRight;
		worldLeft = worldRight = Vector3.zero;
		//Normalize positions
		if(left.offsetZ < right.offsetZ)
		{
			cZMin = cLeft.chunkZ;
			cZMax = cRight.chunkZ;
			worldLeft.z = left.worldZ;
			worldRight.z = right.worldZ;
		}
		else
		{
			cZMin = cRight.chunkZ;
			cZMax = cLeft.chunkZ;
			worldLeft.z = right.worldZ;
			worldRight.z = left.worldZ;
		}
		if (left.offsetX < right.offsetX)
		{
			cXMin = cLeft.chunkX;
			cXMax = cRight.chunkX;
			worldLeft.x = left.worldX;
			worldRight.x = right.worldX;
		}
		else
		{
			cXMin = cRight.chunkX;
			cXMax = cLeft.chunkX;
			worldLeft.x = right.worldX;
			worldRight.x = left.worldX;
		}
		//Fix ensure coords are within bounds
		cXMin = Math.Max(0, cXMin);
		cZMin = Math.Max(0, cZMin);
		cXMax = Math.Min(width, cXMax);
		cZMax = Math.Min(height, cZMax);

#if DEBUG
		Debug.DrawRay(worldLeft, Vector3.up, Color.magenta, 5);
		Debug.DrawLine(worldLeft, worldRight, Color.green, 5);
		Debug.DrawRay(worldRight, Vector3.up, Color.magenta, 5);
#endif

		var selectedUnits = new List<int>();
		for (int cZ = cZMin; cZ <= cZMax; cZ++)
		{
			for (int cX = cXMin; cX <= cXMax; cX++)
			{
				var cIndex = cX + cZ * width;
				if (unitLocations[cIndex] == null || unitLocations[cIndex].Count == 0)
					continue;
				for (int i = 0; i < unitLocations[cIndex].Count; i++)
				{
					var unit = units[unitLocations[cIndex][i]];
					if (unit.Position.x >= worldLeft.x && unit.Position.x <= worldRight.x &&
						unit.Position.z >= worldLeft.z && unit.Position.z <= worldRight.z)
						selectedUnits.Add(unit.id);
				}
			}
		}
		return selectedUnits;
	}

	public List<int> SelectUnitsInRange(HexCoords center, int radius)
	{
		var worldRadius = HexCoords.TileToWorldDist(radius, innerRadius);
		var worldRadiusSq = worldRadius * worldRadius;
		List<int> candidateChunks = new List<int>();
		candidateChunks.Add(center.GetChunkIndex(width));
		var (chunkX, chunkZ) = center.GetChunkPos();
		var centerPos = center.worldXZ;
		var chunkSize = HexCoords.TileToWorldDist(Chunk.SIZE, innerRadius);
		var chunkBounds = Chunks[candidateChunks[0]].Bounds;
		bool left, right, up, down;
		left = right = up = down = false;
		if(centerPos.x - worldRadius < chunkBounds.min.x)
		{
			var nX = chunkX - 1;
			if(left = nX >=0)
				candidateChunks.Add(HexCoords.GetChunkIndex(nX, chunkZ, width));
		}
		if(centerPos.x + worldRadius > chunkBounds.max.x)
		{
			var nX = chunkX + 1;
			if (right = nX <= width)
				candidateChunks.Add(HexCoords.GetChunkIndex(nX, chunkZ, width));
		}

		if (centerPos.z - worldRadius < chunkBounds.min.z)
		{
			var nZ = chunkZ - 1;
			if (down = nZ >= 0)
				candidateChunks.Add(HexCoords.GetChunkIndex(chunkX, nZ, width));
		}
		if (centerPos.z + worldRadius > chunkBounds.max.z)
		{
			var nZ = chunkZ + 1;
			if (up = nZ <= height)
				candidateChunks.Add(HexCoords.GetChunkIndex(chunkX, nZ, width));
		}
		if(up && right)
			candidateChunks.Add(HexCoords.GetChunkIndex(chunkX + 1, chunkZ + 1, width));
		if (down && right)
			candidateChunks.Add(HexCoords.GetChunkIndex(chunkX + 1, chunkZ - 1, width));
		if (up && left)
			candidateChunks.Add(HexCoords.GetChunkIndex(chunkX - 1, chunkZ + 1, width));
		if (down && left)
			candidateChunks.Add(HexCoords.GetChunkIndex(chunkX - 1, chunkZ - 1, width));

		var unitsInRange = new List<int>();
		for (int i = 0; i < candidateChunks.Count; i++)
		{
			var candidateUnits = unitLocations[candidateChunks[i]];
			if (candidateUnits == null)
				continue;
			for (int j = 0; j < candidateUnits.Count; j++)
			{
				var unit = units[candidateUnits[j]];
				var unitPos = new Vector3(unit._position.x, 0, unit._position.z);
				if ((centerPos - unitPos).sqrMagnitude <= worldRadiusSq)
					unitsInRange.Add(unit.id);
			}
		}
		return unitsInRange;
	}

	public TileInfo[] GetTileTyes()
	{
		return Chunks.SelectMany(c => c.Tiles.Select(t => t.info)).Distinct().ToArray();
	}

	internal void UpdateView(Plane[] camPlanes)
	{
		if (!IsRendered)
			throw new Exception("Map is not rendered yet");
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
	}

	/// <summary>
	/// Get Neihboring tiles
	/// </summary>
	/// <param name="coords">Center tile location</param>
	/// <returns>Neihboring tiles arranged clockwise starting from the left</returns>
	public Tile[] GetNeighbors(HexCoords coords)
	{
		Tile[] neighbors = new Tile[6];
		neighbors[0] = this[coords.x - 1, coords.y    ]; //Left
		neighbors[1] = this[coords.x - 1, coords.y + 1]; //Top Left
		neighbors[2] = this[coords.x    , coords.y + 1]; //Top Right
		neighbors[3] = this[coords.x + 1, coords.y    ]; //Right
		neighbors[4] = this[coords.x + 1, coords.y - 1]; //Bottom Right
		neighbors[5] = this[coords.x    , coords.y - 1]; //Bottom Left
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

	public List<Tile> HexSelect(HexCoords center, int radius, bool excludeCenter = false)
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
				var t = this[center.x + x, center.y + y];
				if (t == null)
					continue;
				if (excludeCenter && t.Coords == center)
					continue;
				selection.Add(t);
			}
		}
		return selection;
	}

	public void HexSelectForEach(HexCoords center, int radius, Action<Tile> action, bool excludeCenter = false)
	{
		radius = Mathf.Abs(radius);
		if (radius == 0)
			return;
		for (int y = -radius; y <= radius; y++)
		{
			int xMin = -radius, xMax = radius;
			if (y < 0)
				xMin = -radius - y;
			if (y > 0)
				xMax = radius - y;
			for (int x = xMin; x <= xMax; x++)
			{
				var t = this[center.x + x, center.y + y];
				if (t == null)
					continue;
				if (excludeCenter && t.Coords == center)
					continue;
				action(t);
			}
		}
	}

	public void HexSelectForEach(HexCoords center, int radius, Func<Tile, bool> action, bool excludeCenter = false)
	{
		radius = Mathf.Abs(radius);
		if (radius == 0)
			return;
		bool isDone = false;
		for (int y = -radius; y <= radius; y++)
		{
			if (isDone)
				break;
			int xMin = -radius, xMax = radius;
			if (y < 0)
				xMin = -radius - y;
			if (y > 0)
				xMax = radius - y;
			for (int x = xMin; x <= xMax; x++)
			{
				var t = this[center.x + x, center.y + y];
				if (t == null)
					continue;
				if (excludeCenter && t.Coords == center)
					continue;
				if(!action(t))
				{
					isDone = true;
					break;
				}
			}
		}
	}

	public List<Tile> HexSelectWhere(HexCoords center, int radius, Func<Tile, bool> filter, bool excludeCenter = false)
	{
		radius = Mathf.Abs(radius);
		var selection = new List<Tile>();
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
				var t = this[center.x + x, center.y + y];
				if (t == null)
					continue;
				if (excludeCenter && t.Coords == center)
					continue;
				if (filter(t))
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

	public List<Tile> BoxSelect(HexCoords left, HexCoords right)
	{
		var selection = new List<Tile>();
		int xMin, xMax, zMin, zMax;
		if(left.offsetX < right.offsetX)
		{
			xMin = left.offsetX;
			xMax = right.offsetX;
		}else
		{
			xMax = left.offsetX;
			xMin = right.offsetX;
		}
		if (left.offsetZ < right.offsetZ)
		{
			zMin = left.offsetZ;
			zMax = right.offsetZ;
		}
		else
		{
			zMax = left.offsetZ;
			zMin = right.offsetZ;
		}
		for (int z = zMin; z < zMax; z++)
		{
			for (int x = xMin; x < xMax; x++)
			{
				selection.Add(this[HexCoords.FromOffsetCoords(x, z, tileEdgeLength)]);
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
		innerRadius *= longDiagonal;
		outerRadius *= longDiagonal;
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
			d = MathUtils.Remap(d, 0, (outerRadius * outerRadius) - (innerRadius * innerRadius), 0, 1);
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
			d -= innerRadius * innerRadius * longDiagonal;
			d = MathUtils.Remap(d, 0, (outerRadius * outerRadius * longDiagonal) - (innerRadius * innerRadius * longDiagonal), 0, 1);
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

	public int GetDistance(HexCoords a, HexCoords b)
	{
		var dst = (a.worldXZ - b.worldXZ).magnitude;
		var tileDist = Mathf.RoundToInt(dst / (innerRadius * 2));
		return tileDist;
	}

	public Tile[] GetNeighbors(Tile tile) => GetNeighbors(tile.Coords);

	public List<Tile> GetPath(HexCoords src, HexCoords dst, float maxIncline = float.MaxValue, Func<Tile, bool> include = null) => GetPath(this[src], this[dst], maxIncline, include);

	public List<Tile> GetPath(Tile src, Tile dst, float maxIncline = float.MaxValue, Func<Tile, bool> filter = null)
	{
		if (src == null || dst == null)
			return null;
		if (src == dst)
			return null;
		PathNode BestFScore(HashSet<PathNode> nodes)
		{
			PathNode best = nodes.First();
			foreach (var node in nodes)
			{
				if (best.F > node.F)
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
			var neighbors = GetNeighbors(curTileNode.coords);
			for(int i = 0; i < neighbors.Length; i++)
			{
				if (neighbors[i] == null)
					continue;
				/*if (neighbors[i].IsInBounds(totalHeight, totalWidth))
					continue;*/
				var neighbor = neighbors[i];
				if (Mathf.Abs(neighbor.Height - curTileNode.surfacePoint.y) > maxIncline)
					continue;
				if(filter != null && !neighbor.Equals(dst) && !neighbor.Equals(src) && !filter(neighbor))
				{
					continue;
				}
				var adj = new PathNode(neighbor, curTileNode.G + 1, curTileNode);
				if (closed.Contains(adj))
					continue;
				adj.CacheF(dst.SurfacePoint);
				if (!open.Contains(adj))
					open.Add(adj);
				else
				{
					var o = open.First(oAdj => oAdj.Equals(adj));
					if (adj.F < o.F)
					{
						open.Remove(o);
						open.Add(adj);
					}
				}
			}
			if (open.Count > 512)
			{
				Debug.LogWarning("Big Path");
				break;
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
			path.Add(this[curNode.coords]);
			curNode = curNode.src;
		} while (curNode != null);
		path.Reverse();
		return path;
	}

	private class PathNode
	{
		public HexCoords coords;
		public Vector3 surfacePoint;
		public int G;
		public PathNode src;
		public float F;

		public PathNode(Tile tile, int g, PathNode src = null)
		{
			coords = tile.Coords;
			surfacePoint = tile.SurfacePoint;
			G = g;
			this.src = src;
		}


		public void CacheF(Vector3 b)
		{
			F = CalculateF(b);
		}

		public float CalculateF(Vector3 b)
		{
			var d = surfacePoint - b;
			return G + (d.sqrMagnitude);
		}

		public override bool Equals(object obj)
		{
			if (obj is PathNode n)
			{
				return n.coords.Equals(n.coords);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return coords.GetHashCode();
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
		GC.SuppressFinalize(this);
	}
	#endregion
}
