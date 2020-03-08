using Unity.Entities;
using Unity.Mathematics;

public struct CloudData : IComponentData
{
	public float3 pos;
	public int index;
}

public struct ShadowOnlyTag : IComponentData
{
}