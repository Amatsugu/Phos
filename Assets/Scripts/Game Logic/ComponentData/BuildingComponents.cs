using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

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