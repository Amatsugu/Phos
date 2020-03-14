using System;
using System.Collections.Generic;
using Unity.Collections;
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
	public List<HexCoords> Value;

	public bool Equals(Path other)
	{
		return Value == other.Value;
	}

	public override int GetHashCode()
	{
		return Value?.GetHashCode() ?? 0;
	}
}

public struct PathProgress : IComponentData
{
	public int Delay;
	public int Progress;
}