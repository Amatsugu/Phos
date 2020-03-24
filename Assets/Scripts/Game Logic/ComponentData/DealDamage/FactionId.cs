using Unity.Entities;

public enum Faction
{
	None = 0,
	Player,
	Phos,
	PlayerProjectile,
	PhosProjectile,
	Tile,
	Unit
}

public struct FactionId : IComponentData
{
	public Faction Value;
}