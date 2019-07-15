using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ResearchIdentifier : IComponentData
{
	public BuildingCategory category;
	public int researchId;

	const int prime = 31;
	public override int GetHashCode()
	{
		int hash = 23;
		hash = hash * prime + (int)category;
		hash = hash * prime + researchId;
		return hash;
	}
}
