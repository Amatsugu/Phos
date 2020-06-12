using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;

using DataStore.ConduitGraph;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine;

public class Map : IDisposable
{
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

	public event Action<HexCoords> OnTilePlaced;

	public float seaLevel;
	public int Seed { get; private set; }


	public ConduitGraph conduitGraph;
	public Dictionary<int, MobileUnit> units;
	
	private HashSet<TechBuildingEntity> _techBuildings;
	private int _nextUnitId = 1;
	private MapChunk[] chunks;

	public Map(int height, int width, int seed, float edgeLength = 1)
	{
		this.width = width;
		this.height = height;
		totalWidth = width * MapChunk.SIZE;
		totalHeight = height * MapChunk.SIZE;
		Seed = seed;
		UnityEngine.Random.InitState(seed);
		length = width * height;
		chunks = new MapChunk[length];
		tileEdgeLength = edgeLength;
		innerRadius = Mathf.Sqrt(3f) / 2f * tileEdgeLength;
		shortDiagonal = Mathf.Sqrt(3f) * tileEdgeLength;
		longDiagonal = 2 * tileEdgeLength;
		units = new Dictionary<int, MobileUnit>(500);
		_techBuildings = new HashSet<TechBuildingEntity>();
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
			var chunk = chunks[index];
			if (!chunk.isCreated)
				return null;
			return chunk[coord.ToChunkLocalCoord(chunkX, chunkZ)];
		}

		set
		{
			var (chunkX, chunkZ) = coord.GetChunkPos();
			var chunk = chunks[chunkX + chunkZ * width];
			if (!chunk.isCreated)
				chunk = new MapChunk(this, chunkX, chunkZ, tileEdgeLength, shortDiagonal);
			chunk[coord.ToChunkLocalCoord(chunkX, chunkZ)] = value;
			chunks[chunkX + chunkZ * width] = chunk;
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

	public MobileUnit AddUnit(MobileUnitEntity unitInfo, Tile tile, Faction faction)
	{
		var id = _nextUnitId++;
		var unit = new MobileUnit(id, this, unitInfo, tile, faction);
		units.Add(id, unit);
		if (IsRendered)
			unit.Render();
		return unit;
	}

	public TileEntity[] GetTileTyes()
	{
		return chunks.SelectMany(c => c.Tiles.Select(t => t.info)).Distinct().ToArray();
	}

	internal void UpdateView(Plane[] camPlanes)
	{
		if (!IsRendered)
			throw new Exception("Map is not rendered yet");
		var chunksChanged = 0;

		for (int i = 0; i < chunks.Length; i++)
		{
			if (chunks[i].Show(chunks[i].InView(camPlanes)))
				chunksChanged++;
		}
	}

	public void Render(EntityManager entityManager)
	{
		if (IsRendered)
			return;
		IsRendered = true;
		if (EM == default)
			EM = entityManager;
		
		foreach(var unit in units)
		{
			unit.Value.Render();
		}
	}

	/// <summary>
	/// Get Neihboring tiles
	/// </summary>
	/// <param name="coords">Center tile location</param>
	/// <returns>Neihboring tiles arranged clockwise starting from the left</returns>
	public Tile[] GetNeighbors(HexCoords coords)
	{
		Tile[] neighbors = new Tile[6];
		neighbors[0] = this[coords.X - 1, coords.Y]; //Left
		neighbors[1] = this[coords.X - 1, coords.Y + 1]; //Top Left
		neighbors[2] = this[coords.X, coords.Y + 1]; //Top Right
		neighbors[3] = this[coords.X + 1, coords.Y]; //Right
		neighbors[4] = this[coords.X + 1, coords.Y - 1]; //Bottom Right
		neighbors[5] = this[coords.X, coords.Y - 1]; //Bottom Left
		return neighbors;
	}

	public List<Tile> HexSelect(HexCoords center, int radius, bool excludeCenter = false)
	{
		radius = Mathf.Abs(radius);
		var selection = new List<Tile>(HexCoords.GetTileCount(radius));
		if (radius == 0)
		{
			selection.Add(this[center]);
			return selection;
		}
		var coords = HexCoords.SpiralSelect(center, radius, excludeCenter, innerRadius);
		for (int i = 0; i < coords.Length; i++)
			selection.Add(this[coords[i]]);

		return selection;
	}

	public void HexSelectForEach(HexCoords center, int radius, Action<Tile> action, bool excludeCenter = false)
	{
		radius = Mathf.Abs(radius);
		if (radius == 0)
			return;
		var coords = HexCoords.SpiralSelect(center, radius, excludeCenter, innerRadius);
		for (int i = 0; i < coords.Length; i++)
		{
			var t = this[coords[i]];
			if (t == null)
				continue;
			action(t);
		}
	}

	public void HexSelectForEach(HexCoords center, int radius, Func<Tile, bool> action, bool excludeCenter = false)
	{
		radius = Mathf.Abs(radius);
		if (radius == 0)
			return;
		var coords = HexCoords.SpiralSelect(center, radius, excludeCenter, innerRadius);
		for (int i = 0; i < coords.Length; i++)
		{
			var t = this[coords[i]];
			if (t == null)
				continue;
			if (!action(t))
				break;
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
				var t = this[center.X + x, center.Y + y];
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
				var p = HexCoords.FromPosition(new Vector3(x + center.world.x, 0, z + center.world.z), innerRadius);
				var d = Mathf.Pow(p.world.x - center.world.x, 2) + Mathf.Pow(p.world.z - center.world.z, 2);
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
			var d = math.pow(center.world.x - tile.Coords.world.x, 2) + math.pow(center.world.z - tile.Coords.world.z, 2);
			d -= innerRadius * innerRadius;
			d = MathUtils.Remap(d, 0, (outerRadius * outerRadius) - (innerRadius * innerRadius), 0, 1);
			tile.UpdateHeight(math.lerp(tile.Height, height, 1 - d));
		}
	}

	public void HexFlatten(HexCoords center, int innerRadius, int outerRadius, FlattenMode mode = FlattenMode.Center, bool excludeUnderwater = false)
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
		if (excludeUnderwater)
			outerSelection = outerSelection.Where(t => !t.IsUnderwater);
		foreach (var tile in outerSelection)
		{
			var d = math.pow(center.world.x - tile.Coords.world.x, 2) + Mathf.Pow(center.world.z - tile.Coords.world.z, 2);
			d -= innerRadius * innerRadius * longDiagonal;
			d = MathUtils.Remap(d, 0, (outerRadius * outerRadius * longDiagonal) - (innerRadius * innerRadius * longDiagonal), 0, 1);
			tile.UpdateHeight(math.lerp(tile.Height, height, 1 - d));
		}
	}

	public Tile ReplaceTile(Tile tile, TileEntity newTile)
	{
		if (!IsRendered)
			throw new Exception("Cannot use ReplaceTile for an unrendered map");
		var coord = tile.Coords;
		var (chunkX, chunkZ) = coord.GetChunkPos();
		var index = chunkX + chunkZ * width;
		var localCoord = coord.ToChunkLocalCoord(chunkX, chunkZ);
		var nT = chunks[index].ReplaceTile(localCoord, newTile);
		tile.OnRemoved();
		tile.Destroy();
		if (tile.info is TechBuildingEntity tb && _techBuildings.Contains(tb))
			_techBuildings.Remove(tb);
		nT.OnPlaced();
		if (nT.info is TechBuildingEntity techB && !_techBuildings.Contains(techB))
			_techBuildings.Add(techB);
		OnTilePlaced?.Invoke(coord);
		Debug.Log(JsonConvert.SerializeObject(tile.Coords.world));
		return nT;
	}

	public void RevertTile(Tile tile)
	{
		if (tile.originalTile != null)
			ReplaceTile(tile, tile.originalTile);
		else
			UnityEngine.Debug.LogWarning("No Original Tile to revert to");
	}

	public int GetDistance(HexCoords a, HexCoords b)
	{
		var dst = math.length(a.world - b.world);
		var tileDist = Mathf.RoundToInt(dst / (innerRadius * 2));
		return tileDist;
	}

	public NativeHashMap<HexCoords, float> GenerateNavData()
	{
		var nav = new NativeHashMap<HexCoords, float>(totalHeight * totalWidth, Allocator.Persistent);
		GenerateNavData(ref nav);
		return nav;
	}
	public void GenerateNavData(ref NativeHashMap<HexCoords, float> navData)
	{
		for (int z = 0; z < totalHeight; z++)
		{
			for (int x = 0; x < totalWidth; x++)
			{
				var t = this[HexCoords.FromOffsetCoords(x, z, tileEdgeLength)];
				var navValue = t.IsUnderwater ? t.Height * -1 : (t.info.isTraverseable ? t.Height : float.MinValue);
				navData.Add(t.Coords, navValue);
			}
		}
	}

	public Tile[] GetNeighbors(Tile tile) => GetNeighbors(tile.Coords);

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
		var max = selection.Max(t => t == null ? 0 : t.Height);
		return (max < seaLevel) ? seaLevel : max;
	}

	public SerializedMap Serialize()
	{
		var map = new SerializedMap
		{
			height = height,
			width = width,
			name = Name,
			seaLevel = seaLevel,
			seed = Seed,
			tileEdgeLength = tileEdgeLength,
			tiles = new SerializedTile[length * MapChunk.SIZE * MapChunk.SIZE],
			conduitGrapth = conduitGraph.Serialize()
		};
		int k = 0;
		for (int i = 0; i < chunks.Length; i++)
		{
			for (int j = 0; j < chunks[i].Tiles.Length; j++)
			{
				var curT = chunks[i].Tiles[j];
				map.tiles[k++] = curT.Serialize();
			}
		}
		//TODO: Unit Serialization
		map.units = units.Select(u =>
		{
			var id = GameRegistry.UnitDatabase.entityIds[u.Value.info];
			return new SerializedUnit
			{
				unitId = id,
				pos = EM.GetComponentData<Translation>(u.Value.Entity).Value,
				faction = u.Value.Faction
			};
		}).ToArray();
		return map;
	}

	public void Destroy()
	{
		if (!IsRendered)
			return;
		foreach (var unitEntry in units)
			unitEntry.Value.Destroy();
		foreach (var chunk in chunks)
			chunk.Destroy();
		GameEvents.InvokeOnMapDestroyed();
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
				Destroy();
			}

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

	#endregion IDisposable Support
}

