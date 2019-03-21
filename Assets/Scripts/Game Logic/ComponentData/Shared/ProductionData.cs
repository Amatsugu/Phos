using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct ProductionData : ISharedComponentData
{
	public int[] resourceIds;
	public int[] productionRates;
}

public struct ConsumptionData : ISharedComponentData
{
	public int[] resourceIds;
	public int[] consumptionRates;
}