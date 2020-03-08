using Unity.Entities;
using Unity.Mathematics;

public struct LineSegment : IComponentData
{
	public float3 Start;
	public float3 End;
}