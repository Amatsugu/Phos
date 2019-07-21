using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Research Building")]
public class ResearchBuildingInfo : BuildingTileInfo
{
	public BuildingCategory researchCategory;
	public float consumptionMuli = 2;

	public override IEnumerable<ComponentType> GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[] { typeof(ResearchBuildingCategory), typeof(ResearchConsumptionMulti) });
	}

	public override Entity Instantiate(HexCoords pos, Vector3 scale)
	{
		var e = base.Instantiate(pos, scale);
		Map.EM.SetComponentData(e, new ResearchBuildingCategory { Value = researchCategory });
		Map.EM.SetComponentData(e, new ResearchConsumptionMulti { Value = consumptionMuli });
		return e;
	}

	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new ResearchBuildingTile(pos, height, this);
	}

	public override string GetProductionString()
	{
		var b = base.GetProductionString();
		if (b.Length > 0)
			b += "\n";
		b += $"Unlocks {researchCategory} Research";
		return b;
	}

}
