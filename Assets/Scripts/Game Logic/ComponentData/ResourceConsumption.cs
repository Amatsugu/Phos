using System;

using Unity.Entities;

namespace Amatsugu.Phos
{
	public struct ResourceConsumption : IBufferElementData, IEquatable<ResourceConsumption>
	{
		public int resourceId;
		public float rate;

		public ResourceConsumption(int id, float rate)
		{
			resourceId = id;
			this.rate = rate;
		}

		public bool Equals(ResourceConsumption other) => other.resourceId == resourceId && other.rate == rate;

		public override int GetHashCode()
		{
			int hash = 23;
			hash = hash * 31 + resourceId.GetHashCode();
			hash = hash * 31 * rate.GetHashCode();
			return hash;
		}
	}

	public struct ResourceProduction : IBufferElementData, IEquatable<ResourceProduction>
	{
		public int resourceId;
		public float rate;

		public ResourceProduction(int id, float rate)
		{
			resourceId = id;
			this.rate = rate;
		}

		public bool Equals(ResourceProduction other) => other.resourceId == resourceId && other.rate == rate;

		public override int GetHashCode()
		{
			int hash = 23;
			hash = hash * 31 + resourceId.GetHashCode();
			hash = hash * 31 * rate.GetHashCode();
			return hash;
		}
	}
}