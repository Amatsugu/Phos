using System;
using System.Collections.Generic;

using Unity.Entities;
using Unity.Mathematics;

public struct MoveSpeed : IComponentData
{
	public float Value;
}

public struct Heading : IComponentData
{
	public float3 Value;
}

public struct Destination : IComponentData
{
	public float3 Value;
}

public struct UnitId : IComponentData
{
	public int Value;
}

public struct NextTile : IComponentData
{
	public float3 Value;
}

public struct Path : ISharedComponentData, IEquatable<Path>
{
	public List<Tile> Value;

	public bool Equals(Path other)
	{
		return Value == other.Value;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}

public struct PathProgress : IComponentData
{
	public int Delay;
	public int Progress;
}