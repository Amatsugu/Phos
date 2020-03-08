using Unity.Entities;

public struct ResearchIdentifier : IComponentData
{
	public BuildingCategory category;
	public int researchId;

	private const int prime = 31;

	public override int GetHashCode()
	{
		int hash = 23;
		hash = hash * prime + (int)category;
		hash = hash * prime + researchId;
		return hash;
	}
}