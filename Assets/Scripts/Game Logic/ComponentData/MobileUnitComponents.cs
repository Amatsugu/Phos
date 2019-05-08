using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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