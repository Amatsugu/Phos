using System;

using Unity.Entities;

public struct ProductionData : ISharedComponentData, IEquatable<ProductionData>
{
	public int[] resourceIds;
	public int[] rates;

	public bool Equals(ProductionData other)
	{
		return resourceIds == other.resourceIds && rates == other.rates;
	}

	public override int GetHashCode()
	{
		int hash = 23;
		hash = hash * 31 + rates.GetHashCode();
		hash = hash * 31 * resourceIds.GetHashCode();
		return hash;
	}
}

public struct ConsumptionData : ISharedComponentData, IEquatable<ConsumptionData>
{
	public int[] resourceIds;
	public int[] rates;

	public bool Equals(ConsumptionData other)
	{
		return resourceIds == other.resourceIds && rates == other.rates;
	}

	public override int GetHashCode()
	{
		int hash = 23;
		hash = hash * 31 + rates.GetHashCode();
		hash = hash * 31 * resourceIds.GetHashCode();
		return hash;
	}
}