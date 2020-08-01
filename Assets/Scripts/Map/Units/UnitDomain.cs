using System;
using Unity.Entities;

[Serializable]
public static class UnitDomain
{
	public struct Air : IComponentData
	{}
	public struct Land : IComponentData
	{ }
	public struct Naval : IComponentData
	{ }

	[Flags]
	public enum Domain
	{
		Land=1,
		Air=2,
		Naval=4
	}
}

public struct TagetingDomain : IComponentData
{
	public UnitDomain.Domain Value;
}


[Serializable]
public static class UnitClass 
{
	public struct FixedGun : IComponentData{ }
	public struct Turret : IComponentData{ }
	public struct Artillery : IComponentData{ }
	public struct Support : IComponentData{ }
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
