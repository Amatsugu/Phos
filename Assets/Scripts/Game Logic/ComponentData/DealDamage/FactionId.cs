using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public enum Faction
{
	None,
	Player,
	Phos
}

public struct FactionId : IComponentData
{
	public Faction Value;
}
