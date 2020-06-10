using System;
using Unity.Entities;

[Serializable]
public struct UnitDomain : IComponentData, IEquatable<UnitDomain>
{
	public Domain Value;

	public override bool Equals(object obj)
	{
		return obj is UnitDomain domain && Equals(domain);
	}

	public bool Equals(UnitDomain other) => Value == other.Value;

	public override int GetHashCode() => -1937169414 + Value.GetHashCode();

	public static bool operator ==(UnitDomain left, UnitDomain right) => left.Equals(right);

	public static bool operator !=(UnitDomain left, UnitDomain right) => !(left == right);

	public enum Domain
	{
		Land,
		Air,
		Naval
	}
}


[Serializable]
public struct UnitClass : IComponentData, IEquatable<UnitClass>
{
	public Class Value;

	public override bool Equals(object obj) => obj is UnitClass @class && Equals(@class);

	public bool Equals(UnitClass other) => Value == other.Value;

	public override int GetHashCode() => -1937169414 + Value.GetHashCode();

	public static bool operator ==(UnitClass left, UnitClass right) => left.Equals(right);

	public static bool operator !=(UnitClass left, UnitClass right) => !(left == right);

	public enum Class
	{
		FixedGun,
		Turret,
		Artillery,
		Support
	}
}

public struct UnitState : IComponentData
{
	public enum State
	{
		AttackOnSight,
		HoldFire,
	}

	public State Value;
}
