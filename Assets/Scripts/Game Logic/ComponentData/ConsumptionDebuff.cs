using Unity.Entities;

public struct ConsumptionDebuff : IComponentData
{
	public const float multi = 0.5f;
	public int distance;
}