using Unity.Entities;

public struct Health : IComponentData
{
	public float Value;
	public float maxHealth;
}

public struct HealthRegen : IComponentData
{
	public float Value;
}

public struct Damage : IComponentData
{
	public float Value;
	public bool friendlyFire;
}