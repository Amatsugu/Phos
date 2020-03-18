using Unity.Entities;

public enum Faction
{
	None = 0,
	Player,
	Phos
}

public struct FactionId : IComponentData
{
	public Faction Value;
}