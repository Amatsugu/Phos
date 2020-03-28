using Unity.Entities;

public struct Health : IComponentData
{
	public float Value;
	public float maxHealth;
}

public struct Damage : IComponentData
{
	public float Value;
	public bool friendlyFire;
}