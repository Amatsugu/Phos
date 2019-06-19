using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HexCoords
{
	//Position
	[SerializeField]
	public readonly int x;
	[SerializeField]
	public readonly int y;
	[SerializeField]
	public readonly int z;
	//Hex info
	[HideInInspector]
	public readonly float edgeLength;
	//World Pos
	public readonly float worldX;
	public readonly float worldZ;
	public readonly Vector3 worldXZ;
	public readonly Vector2 worldXY;
	//Offsets
	public readonly int offsetX;
	public readonly int offsetZ;
	public readonly bool isCreated;

	public HexCoords(int x, int y, float edgeLength)
	{
		this.x = x;
		this.y = y;
		this.z = -x - y;
		this.edgeLength = edgeLength;
		var innerRadius = Mathf.Sqrt(3f) / 2f * this.edgeLength;
		offsetX = x + y / 2;
		offsetZ = y;
		//worldX = (offsetX + offsetZ * .5f - offsetZ / 2) * (innerRadius * 2f);
		//worldZ = offsetZ * (this.edgeLength * 1.5f);
		(worldX, worldZ) = OffsetToWorldPos(offsetX, offsetZ, innerRadius, edgeLength);

		worldXZ = new Vector3(worldX, 0, worldZ);
		worldXY = new Vector2(worldX, worldZ);
		isCreated = true;
	}

	public static Vector3 SnapToGrid(Vector3 worldPos, float innerRadius, float edgeLength)
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

	public static HexCoords FromOffsetCoords(int x, int Z, float edgeLength) => new HexCoords(x - (Z / 2), Z, edgeLength);

	public static HexCoords FromPosition(Vector3 position, float edgeLength = 1)
	{
		float innerRadius = Mathf.Sqrt(3f) / 2f * edgeLength;
		float x = position.x / (innerRadius * 2f);
		float z = -x;
		float offset = position.z / (edgeLength * 3f);
		z -= offset;
		x -= offset;
		int iX = Mathf.RoundToInt(x);
		int iY = Mathf.RoundToInt(-x -z);
		return new HexCoords(iX, iY, edgeLength);
	}

	public HexCoords ToChunkLocalCoord()
	{
		var (x, z) = GetChunkPos();
		return ToChunkLocalCoord(x, z);
	}

	public HexCoords ToChunkLocalCoord(int chunkX, int chunkZ) => FromOffsetCoords(offsetX - (chunkX * Map.Chunk.SIZE), offsetZ - (chunkZ * Map.Chunk.SIZE), edgeLength);

	public (int chunkX, int chunkZ) GetChunkPos() => (Mathf.FloorToInt((float)offsetX / Map.Chunk.SIZE), Mathf.FloorToInt((float)offsetZ / Map.Chunk.SIZE));

	public int GetChunkIndex(int width)
	{
		var (cx, cz) = GetChunkPos();
		return cx + cz * width;
	}

	public int ToIndex(int mapWidth) => x + y * mapWidth + y / 2;

	
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

	public override string ToString() => $"({x}, {y}, {z})";

	public static bool operator ==(HexCoords a, HexCoords b) => a.Equals(b);

	public static bool operator !=(HexCoords a, HexCoords b) => !a.Equals(b);

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
		return (h.x == x && h.y == y);
	}

	// override object.GetHashCode
	const int prime = 31;
	public override int GetHashCode()
	{
		int hash = 23;
		hash = hash * prime + offsetX;
		hash = hash * prime * offsetZ;
		return hash;
	}
}
