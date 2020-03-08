using Unity.Entities;

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