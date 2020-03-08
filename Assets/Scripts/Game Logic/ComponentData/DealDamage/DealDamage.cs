using Unity.Entities;

public class DealDamage : IComponentData
{
	public float damage;
	public Faction src;
}