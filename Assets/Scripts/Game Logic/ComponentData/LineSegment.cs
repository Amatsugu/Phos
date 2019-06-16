using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct LineSegment : IComponentData
{
	public float3 Start;
	public float3 End;
}
