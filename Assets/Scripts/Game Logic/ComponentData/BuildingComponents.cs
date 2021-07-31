using System.CodeDom.Compiler;

using Unity.Entities;
using Unity.Mathematics;

public struct InactiveBuildingTag : IComponentData
{
}

public struct BuildingOffTag : IComponentData
{
}

public struct BuildingDisabledTag : IComponentData
{ }

public struct FirstTickTag : IComponentData
{
}

public struct BuildingId : IComponentData
{
	public int Value;
}

public struct Building : IComponentData
{
	public Entity Value;

	public static implicit operator Entity(Building building) => building.Value;
	public static implicit operator Building(Entity buildingInst) => new Building {Value = buildingInst };
}

public struct BuildingInitTag : IComponentData
{

}

public struct BuildingBonusInitTag : IComponentData
{

}