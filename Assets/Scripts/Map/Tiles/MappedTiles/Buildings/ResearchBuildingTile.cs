using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchBuildingTile : PoweredBuildingTile
{
	public ResearchBuildingInfo researchInfo;

	public ResearchBuildingTile(HexCoords coords, float height, ResearchBuildingInfo tInfo) : base(coords, height, tInfo)
	{
		researchInfo = tInfo;
	}

	protected override void OnBuilt()
	{
		base.OnBuilt();
		ResearchSystem.UnlockCategory(researchInfo.researchCategory);
	}
}
