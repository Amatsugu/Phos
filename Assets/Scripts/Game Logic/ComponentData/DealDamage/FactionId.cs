using System;
using Unity.Entities;

public enum Faction
{
	None,
	Player,
	Phos
}

public struct FactionId : IComponentData, IEquatable<FactionId>
{
	public Faction Value;

	public bool Equals(FactionId other) => Value == other.Value;

	public override bool Equals(object obj)
	{
		if (obj is FactionId id)
			return Equals(id);
		return false;
	}

	public override int GetHashCode() => Value.GetHashCode();

	public static bool operator ==(FactionId left, FactionId right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(FactionId left, FactionId right)
	{
		return !(left == right);
	}
}