using System.Collections.Generic;
using System.Linq;

using Unity.Entities;

using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Research Building")]
public class ResearchBuildingInfo : BuildingTileEntity
{
	public BuildingCategory researchCategory;
	public float consumptionMuli = 2;

	public override Tile CreateTile(Map map, HexCoords pos, float height)
	{
		return new ResearchBuildingTile(pos, height, map, this);
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