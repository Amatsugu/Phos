using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ResearchIdentifier : IComponentData
{
	public BuildingCategory category;
	public int researchId;
}
