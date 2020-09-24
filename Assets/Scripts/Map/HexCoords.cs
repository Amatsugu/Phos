using Newtonsoft.Json;

using System;

using Unity.Mathematics;

using UnityEngine;

[Serializable]
public struct HexCoords : IEquatable<HexCoords>
{
	//Position
	[SerializeField]
	public readonly int X;

	[SerializeField]
	public readonly int Y;

	[JsonIgnore]
	public int Z => -X - Y;

	//Hex info
	[HideInInspector]
	public readonly float edgeLength;

	//World Pos
	[JsonIgnore]
	public float3 WorldPos {
		get
		{
			var (worldX, worldZ) = OffsetToWorldPos(OffsetCoords.x, OffsetCoords.y, HexCoords.CalculateInnerRadius(edgeLength), edgeLength);
			return new float3(worldX, 0, worldZ);
		}
	}

	//Offsets
	[JsonIgnore]
	public int2 OffsetCoords => new int2(X + Y /2, Y);

	public readonly bool isCreated;

	public HexCoords(int x, int y, float edgeLength)
	{
		this.X = x;
		this.Y = y;
		this.edgeLength = edgeLength;
		isCreated = true;
	}

	public static float3 SnapToGrid(Vector3 worldPos, float innerRadius, float edgeLength)
	{
		float x = worldPos.x / (innerRadius * 2f);
		float z = -x;
		float offset = worldPos.z / (edgeLength * 3f);
		z -= offset;
		x -= offset;
		int iX = Mathf.RoundToInt(x);
		int iY = Mathf.RoundToInt(-x - z);
		var offsetX = iX + iY / 2;
		var offsetZ = iY;
		return OffsetToWorldPosXZ(offsetX, offsetZ, innerRadius, edgeLength);
	}

	public static float CalculateInnerRadius(float edgeLength) => Mathf.Sqrt(3f) / 2f * edgeLength;

	public static float CalculateShortDiagonal(float edgeLength) => Mathf.Sqrt(3f) * edgeLength;

	public static HexCoords FromOffsetCoords(int x, int Z, float edgeLength = 1) => new HexCoords(x - (Z / 2), Z, edgeLength);

	public static HexCoords FromPosition(Vector3 position, float edgeLength = 1)
	{
		float innerRadius = CalculateInnerRadius(edgeLength);
		float x = position.x / (innerRadius * 2f);
		float z = -x;
		float offset = position.z / (edgeLength * 3f);
		z -= offset;
		x -= offset;
		int iX = Mathf.RoundToInt(x);
		int iY = Mathf.RoundToInt(-x - z);
		return new HexCoords(iX, iY, edgeLength);
	}

	public HexCoords ToChunkLocalCoord()
	{
		var (x, z) = GetChunkPos();
		return ToChunkLocalCoord(x, z);
	}

	public HexCoords TranslateOffset(int x, int z) => FromOffsetCoords(OffsetCoords.x + x, OffsetCoords.y + z, edgeLength);

	public HexCoords ToChunkLocalCoord(int chunkX, int chunkZ) => FromOffsetCoords(OffsetCoords.x - (chunkX * MapChunk.SIZE), OffsetCoords.y - (chunkZ * MapChunk.SIZE), edgeLength);

	public (int chunkX, int chunkZ) GetChunkPos() => (Mathf.FloorToInt((float)OffsetCoords.x / MapChunk.SIZE), Mathf.FloorToInt((float)OffsetCoords.y / MapChunk.SIZE));

	public int GetChunkIndex(int width)
	{
		var (cx, cz) = GetChunkPos();
		return cx + cz * width;
	}

	public static int GetChunkIndex(int chunkX, int chunkZ, int width) => chunkX + chunkZ * width;

	public int ToIndex(int mapWidth) => X + Y * mapWidth + Y / 2;

	public int Distance(HexCoords b) => (math.abs(X - b.X) + math.abs(Y - b.Y) + math.abs(Z - b.Z)) / 2;

	public float DistanceToSq(HexCoords b) => math.lengthsq(WorldPos - b.WorldPos);

	public static float DistanceSq(HexCoords a, HexCoords b) => math.lengthsq(a.WorldPos - b.WorldPos);

	public static (float X, float Z) OffsetToWorldPos(int x, int z, float innerRadius, float edgeLength)
	{
		var worldX = (x + z * .5f - z / 2) * (innerRadius * 2f);
		var worldZ = z * (edgeLength * 1.5f);
		return (worldX, worldZ);
	}

	public static Vector3 OffsetToWorldPosXZ(int x, int z, float innerRadius, float edgeLength)
	{
		var (wX, wZ) = OffsetToWorldPos(x, z, innerRadius, edgeLength);
		return new Vector3(wX, 0, wZ);
	}

	public static float TileToWorldDist(int tileCount, float innerRadius) => tileCount * (2 * innerRadius);

	public static int WorldToTileDist(float dist, float innerRadius) => Mathf.RoundToInt(dist / (2 * innerRadius));

	public override string ToString() => $"({X}, {Y}, {Z})";

	public static int GetTileCount(int r) => (1 + 3 * (r + 1) * (r));

	public static int CalculateRadius(int tileCount)
	{
		var sqrt = math.sqrt(3 * ((4 * tileCount) - 1));
		var a = -(3 + sqrt) / 6f;
		var b = -(3 - sqrt) / 6f;
		if (a < 0)
			return Mathf.CeilToInt(b);
		else
			return Mathf.CeilToInt(a);
	}
	[Obsolete]
	public static HexCoords[] HexSelect(HexCoords center, int radius, bool excludeCenter = false)
	{
		radius = Mathf.Abs(radius);
		var i = 0;
		if (radius == 0)
		{
			return new HexCoords[] { center };
		}
		var selection = new HexCoords[GetTileCount(radius)];
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
				var coord = new HexCoords(center.X + x, center.Y + y, center.edgeLength);
				if (excludeCenter && coord == center)
					continue;
				selection[i++] = coord;
			}
		}
		return selection;
	}

	public static HexCoords[] SpiralSelect(HexCoords center, int radius, bool excludeCenter = false)
	{
		int count = GetTileCount(radius);
		if (excludeCenter)
			count--;
		var selection = new HexCoords[count];
		int c = 0;
		if (!excludeCenter)
			selection[c++] = center;
		for (int k = 0; k <= radius; k++)
		{
			var item = center.Scale(4, k);
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < k; j++)
				{
					selection[c++] = item;
					item = item.GetNeighbor(i);
				}
			}
		}
		return selection;
	}

	public static HexCoords[] SelectRing(HexCoords center, int radius)
	{
		var items = new HexCoords[6 * radius];
		int c = 0;
		var item = center.Scale(4, radius);
		for (int i = 0; i < 6; i++)
		{
			for (int j = 0; j < radius; j++)
			{
				items[c++] = item;
				item = item.GetNeighbor(i);
			}
		}
		return items;
	}

	public static readonly int3[] DIRECTIONS = new int3[]
	{
		new int3( 1, -1,  0),
		new int3( 1,  0, -1),
		new int3( 0, +1, -1),
		new int3(-1, +1,  0),
		new int3(-1,  0, +1),
		new int3( 0, -1, +1),
	};

	public HexCoords RotateAround(HexCoords center, int angle) => Rotate(this, center, angle);

	public static HexCoords Rotate(HexCoords coord, HexCoords center, int angle)
	{
		if (coord == center)
			return coord;
		if (angle >= 6)
			angle /= 6;
		if (angle == 0)
			return coord;
		var pc = new int3(coord.X - center.X, coord.Y - center.Y, coord.Z - center.Z);
		if(angle > 0)
		{
			for (int i = 0; i < angle; i++)
				pc = SlideRight(pc);
		}else
		{
			angle = math.abs(angle);
			for (int i = 0; i < angle; i++)
				pc = SlideLeft(pc);
		}
		var r = new HexCoords(pc.x + center.X, pc.y + center.Y, coord.edgeLength);
		var eR = new int3(pc.x + center.X, pc.y + center.Y, pc.z + center.Z);
		return r;
	}

	private static int3 SlideLeft(int3 point)
	{
		return new int3(-point.y, -point.z, -point.x);
	}

	private static int3 SlideRight(int3 point)
	{
		return new int3(-point.z, -point.x, -point.y);
	}

	public static HexCoords Zero(float edgeLength)
	{
		return new HexCoords(0, 0, edgeLength);
	}

	public HexCoords Scale(int dir, int radius)
	{
		var s = DIRECTIONS[dir] * radius;
		return new HexCoords(X + s.x, Y + s.y, edgeLength);
	}

	public HexCoords GetNeighbor(int dir)
	{
		var d = DIRECTIONS[dir];
		return new HexCoords(X + d.x, Y + d.y, edgeLength);
	}

	public static HexCoords[] GetNeighbors(HexCoords center)
	{
		HexCoords[] neighbors = new HexCoords[6];
		for (int i = 0; i < 6; i++)
			neighbors[i] = center.GetNeighbor(i);
		return neighbors;
	}

	public bool IsInBounds(int height, int widht)
	{
		if (0 > OffsetCoords.y || height <= OffsetCoords.y)
			return false;
		if (0 > OffsetCoords.x || widht <= OffsetCoords.x)
			return false;
		return true;
	}
	// override object.GetHashCode
	private const int prime = 31;

	public override int GetHashCode()
	{
		int hash = 23;
		hash = hash * prime + OffsetCoords.x;
		hash = hash * prime + OffsetCoords.y;
		return hash;
	}

	public static bool operator !=(HexCoords a, HexCoords b) => !a.Equals(b);

	public static bool operator ==(HexCoords a, HexCoords b) => a.Equals(b);

	// override object.Equals
	public override bool Equals(object obj)
	{
		if (!isCreated)
			return false;
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}

		var h = (HexCoords)obj;
		if (!h.isCreated)
			return false;
		return this.Equals(h);
	}

	public bool Equals(HexCoords other) => isCreated && other.isCreated && X == other.X && Y == other.Y;
}