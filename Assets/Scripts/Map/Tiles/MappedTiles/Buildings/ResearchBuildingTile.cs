public class ResearchBuildingTile : PoweredBuildingTile
{
	public ResearchBuildingInfo researchInfo;

	public ResearchBuildingTile(HexCoords coords, float height, ResearchBuildingInfo tInfo) : base(coords, height, tInfo)
	{
		researchInfo = tInfo;
	}

	protected override void PrepareEntity()
	{
		base.PrepareEntity();
		var e = GetBuildingEntity();
		Map.EM.AddComponentData(e, new ResearchBuildingCategory { Value = researchInfo.researchCategory });
		Map.EM.AddComponentData(e, new ResearchConsumptionMulti { Value = researchInfo.consumptionMuli });
	}

	protected override void OnBuilt()
	{
		base.OnBuilt();
		ResearchSystem.UnlockCategory(researchInfo.researchCategory);
	}
}