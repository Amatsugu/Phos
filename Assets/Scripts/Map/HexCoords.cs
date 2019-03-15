using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct HexCoords
{
	public int X { get; private set; }
	public int Y { get; private set; }
	public int Z
	{
		get
		{
			return -X - Y;
		}
	}

	public float EdgeLength { get; }
	public float InnerRadius => Mathf.Sqrt(3f) / 2f * EdgeLength;
	public float ShortDiagonal => Mathf.Sqrt(3f) * EdgeLength;

	public float WorldX => (OffsetX + OffsetZ * .5f - OffsetZ / 2) * (InnerRadius * 2f);
	public float WorldZ => OffsetZ * (EdgeLength * 1.5f);


	public Vector3 WorldXZ => new Vector3(WorldX, 0, WorldZ);
	public Vector2 WorldXY => new Vector2(WorldX, WorldZ);

	public HexCoords(int x, int y, float edgeLength)
	{
		X = x;
		Y = y;
		EdgeLength = edgeLength;
	}

	public static HexCoords FromOffsetCoords(int x, int Z, float edgeLength)
	{
		return new HexCoords(x - Z / 2, Z, edgeLength);
	}

	public int OffsetX => X + Y / 2;
	public int OffsetZ => Y;

	public static HexCoords FromPosition(Vector3 position, float edgeLength = 1)
	{
		float innerRadius = Mathf.Sqrt(3f) / 2f * edgeLength;
		float x = position.x / (innerRadius * 2f);
		float z = -x;
		float offset = position.z / (innerRadius * 3f);
		z -= offset;
		x -= offset;
		int iX = Mathf.RoundToInt(x);
		int iZ = Mathf.RoundToInt(z);
		int iY = Mathf.RoundToInt(-x -z);
		//if (iX + iY + iZ != 0)
			//Debug.LogWarning("Rounding error");
		return new HexCoords(iX, iY, edgeLength);
	}

	public int ToIndex(int mapWidth)
	{
		return X + Y * mapWidth + Y / 2;
	}

	public override string ToString()
	{
		return $"({X}, {Y}, {Z})";
	}

	public static bool operator ==(HexCoords a, HexCoords b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(HexCoords a, HexCoords b)
	{
		return !a.Equals(b);
	}

	// override object.Equals
	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}

		var h = (HexCoords)obj;
		return (h.X == X && h.Y == Y);
	}

	// override object.GetHashCode
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
