using Unity.Entities;
using Unity.Mathematics;

public struct InactiveBuildingTag : IComponentData
{
}

public struct BuildingOffTag : IComponentData
{
}

public struct FirstTickTag : IComponentData
{
}

public struct BuildingId : IComponentData
{
	public int Value;
}

public struct CenterOfMassOffset : IComponentData
{
	public float3 Value;
}