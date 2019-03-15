using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class Map<T> where T : Tile
{

	public int Height { get; }
	public int Width { get; }
	public int Length => Chunks.Length;
	public float TileEdgeLength { get; }
	public float LongDiagonal => 2 * TileEdgeLength;
	public float ShortDiagonal => Mathf.Sqrt(3f) * TileEdgeLength;

	public float InnerRadius => Mathf.Sqrt(3f) / 2f * TileEdgeLength;
	public float SeaLevel;
	public Transform Parent { get; }
	public Chunk[] Chunks { get; }


	public class Chunk
	{
		public const int SIZE = 16;
		public T[] Tiles;
		public HexCoords chunkCoord;
		public bool isShown = false;

		private Bounds _bound;
		private GameObject _chunkObject;

		public Chunk(HexCoords coord)
		{
			chunkCoord = coord;
			Tiles = new T[SIZE * SIZE];
			var worldCoord = HexCoords.FromOffsetCoords(coord.OffsetX * SIZE, coord.OffsetZ * SIZE, coord.EdgeLength);
			_bound = new Bounds
			{
				min = worldCoord.WorldXZ,
				max = worldCoord.WorldXZ + new Vector3(SIZE * coord.ShortDiagonal, 0, SIZE * 1.5f)
			};
		}

		public T this[HexCoords coord]
		{
			get
			{
				return Tiles[coord.ToIndex(SIZE)];
			}

			set => Tiles[coord.ToIndex(SIZE)] = value;
		}

		public bool InView(Plane[] camPlanes)
		{
			var inView = GeometryUtility.TestPlanesAABB(camPlanes, _bound);
			var color = inView ? Color.red : Color.blue;
			Debug.DrawLine(new Vector3(_bound.min.x, 0, _bound.min.z), new Vector3(_bound.min.x, 0, _bound.max.z), color);
			Debug.DrawLine(new Vector3(_bound.min.x, 0, _bound.max.z), new Vector3(_bound.max.x, 0, _bound.max.z), color);
			Debug.DrawLine(new Vector3(_bound.max.x, 0, _bound.max.z), new Vector3(_bound.max.x, 0, _bound.min.z), color);
			Debug.DrawLine(new Vector3(_bound.min.x, 0, _bound.min.z), new Vector3(_bound.max.x, 0, _bound.min.z), color);
			Debug.DrawLine(new Vector3(_bound.min.x, 0, _bound.min.z), new Vector3(_bound.min.x, SIZE, _bound.min.z), color);
			return inView;
		}

		public void Render(Transform parent)
		{
			isShown = true;
			_chunkObject = new GameObject();
			_chunkObject.transform.position = HexCoords.FromOffsetCoords(chunkCoord.OffsetX * SIZE, chunkCoord.OffsetZ * SIZE, chunkCoord.EdgeLength).WorldXZ;
			_chunkObject.transform.parent = parent;
			_chunkObject.name = $"Chunk[{chunkCoord.OffsetX}, {chunkCoord.OffsetZ}]";
			foreach (var tile in Tiles)
				tile.Render(_chunkObject.transform);
		}

		public void Destroy()
		{
			UnityEngine.Object.Destroy(_chunkObject);
			foreach (var tile in Tiles)
				tile.Destroy();
		}

		public bool Show(bool shown)
		{
			if (shown == isShown)
				return false;
			isShown = shown;
			_chunkObject.SetActive(isShown);
			//foreach (var tile in Tiles)
				//tile.Show(isShown);
			return true;
		}

		internal void Render(EntityManager entityManager)
		{
			foreach (var tile in Tiles)
			{
				tile.Render(entityManager);
			}
		}
	}

	public Map(int height, int width, Transform parent, float edgeLength = 1)
	{
		Width = width;
		Height = height;
		Chunks = new Chunk[width * height];
		TileEdgeLength = edgeLength;
		Parent = parent;
	}

	/// <summary>
	/// Get a tile the given HexCoords position
	/// </summary>
	/// <param name="coord">Position</param>
	/// <returns>Tile at position</returns>
	public T this[HexCoords coord]
	{
		get
		{
			var chunkX = Mathf.FloorToInt((float)coord.OffsetX / Chunk.SIZE);
			var chunkZ = Mathf.FloorToInt((float)coord.OffsetZ / Chunk.SIZE);
			var index = chunkX + chunkZ * Width;
			if (index < 0 || index > Length)
				return null;
			var chunk = Chunks[index];
			if (chunk == null)
				return null;
			return chunk[HexCoords.FromOffsetCoords(coord.OffsetX - (chunkX * Chunk.SIZE), coord.OffsetZ - (chunkZ * Chunk.SIZE), TileEdgeLength)];
		}

		set
		{
			var chunkX = coord.OffsetX / Chunk.SIZE;
			var chunkZ = coord.OffsetZ / Chunk.SIZE;
			var chunk = Chunks[chunkX + chunkZ * Width];
			if (chunk == null)
				chunk = Chunks[chunkX + chunkZ * Width] = new Chunk(HexCoords.FromOffsetCoords(chunkX, chunkZ, TileEdgeLength));
			chunk[HexCoords.FromOffsetCoords(coord.OffsetX - chunkX * Chunk.SIZE, coord.OffsetZ - chunkZ * Chunk.SIZE, TileEdgeLength)] = value;
		}
	}


	/// <summary>
	/// Get a tile at the given HexCoords Position
	/// </summary>
	/// <param name="x">X component</param>
	/// <param name="y">Y component</param>
	/// <param name="z">Z component</param>
	/// <returns>Tile at position</returns>
	public T this[int x, int y, int z]
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

	public void Render(Transform parent)
	{
		foreach (var chunk in Chunks)
			chunk?.Render(parent);
	}

	public void Render(EntityManager entityManager)
	{
		//EntityArchetype tileArchetype = entityManager.CreateArchetype(
		//	typeof(Translation),
		//	typeof(Rotation),
		//	typeof(NonUniformScale)
		//	);
		//var tileEntity = entityManager.CreateEntity(tileArchetype);
		//Dictionary<TileInfo, Entity> tileEntities = new Dictionary<TileInfo, Entity>();
		//var tileTypes = GetTileTyes();
		//foreach (var tInfo in tileTypes)
			//tileEntities.Add(tInfo, GameObjectConversionUtility.ConvertGameObjectHierarchy(tInfo.tilePrefab, entityManager.World));
		foreach (var chunk in Chunks)
		{
			chunk.Render(entityManager);
		}
	}

	public T[] GetNeighbors(HexCoords coords)
	{
		T[] neighbors = new T[6];
		neighbors[0] = this[coords.X - 1, coords.Y, coords.Z + 1]; //Left
		neighbors[1] = this[coords.X - 1, coords.Y + 1, coords.Z]; //Top Left
		neighbors[2] = this[coords.X, coords.Y + 1, coords.Z - 1]; //Top Right
		neighbors[3] = this[coords.X + 1, coords.Y, coords.Z - 1]; //Right
		neighbors[4] = this[coords.X + 1, coords.Y - 1, coords.Z]; //Bottom Right
		neighbors[5] = this[coords.X, coords.Y - 1, coords.Z + 1]; //Bottom Left
		return neighbors;
	}


	public List<T> HexSelect(HexCoords center, int radius)
	{
		var selection = new List<T>();
		radius = Mathf.Abs(radius);
		if(radius == 0)
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
				var t = this[center.X + x, center.Y + y, center.Z + z];
				if(t != null)
					selection.Add(t);
			}
		}
		return selection;
	}

	public List<T> CircularSelect(HexCoords center, float radius)
	{
		var selection = new List<T>();
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
				var p = HexCoords.FromPosition(new Vector3(x + center.WorldX, 0, z + center.WorldZ), InnerRadius);
				var d = Mathf.Pow(p.WorldX - center.WorldX, 2) + Mathf.Pow(p.WorldZ - center.WorldZ, 2);
				if(d <= radius * radius)
				{
					var t = this[p]; 
					if(t != null)
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
		if(mode == FlattenMode.Average)
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
			var d = Mathf.Pow(center.WorldX - tile.Coords.WorldX, 2) + Mathf.Pow(center.WorldZ - tile.Coords.WorldZ, 2);
			d -= innerRadius * innerRadius;
			d = MathUtils.Map(d, 0, (outerRadius * outerRadius) - (innerRadius * innerRadius), 0, 1);
			tile.UpdateHeight(Mathf.Lerp(tile.Height, height, 1-d));
		}
	}

	public void HexFlatten(HexCoords center, int innerRadius, int outerRadius, FlattenMode mode = FlattenMode.Center)
	{
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
			var d = Mathf.Pow(center.WorldX - tile.Coords.WorldX, 2) + Mathf.Pow(center.WorldZ - tile.Coords.WorldZ, 2);
			d -= innerRadius * innerRadius * LongDiagonal;
			d = MathUtils.Map(d, 0, (outerRadius * outerRadius * LongDiagonal) - (innerRadius * innerRadius * LongDiagonal), 0, 1);
			tile.UpdateHeight(Mathf.Lerp(tile.Height, height, 1 - d));
		}
	}

	public void ReplaceTile(HexCoords tilePos, T newTile)
	{
		this[tilePos].Destroy();
		this[tilePos] = newTile;
		newTile.Render(Parent);
	}

	public T[] GetNeighbors(T tile) => GetNeighbors(tile.Coords);

	public void Destroy()
	{
		foreach (var chunk in Chunks)
			chunk?.Destroy();
	}
}
