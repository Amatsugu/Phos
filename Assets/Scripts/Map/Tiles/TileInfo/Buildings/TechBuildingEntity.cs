using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Tech Building")]
public class TechBuildingEntity : BuildingTileEntity
{
	[Header("Tech Up")]
	public BuildingIdentifier[] buildingsToUnlock;

	public override Tile CreateTile(Map map, HexCoords pos, float height)
	{
		return new TechBuildingTile(pos, height, map, this);
	}

	public override string GetProductionString()
	{
		var prod = new StringBuilder();
		prod.AppendLine("Unlocks Buildings:");
		for (int i = 0; i < buildingsToUnlock.Length; i++)
		{
			prod.Append("\t");
			prod.AppendLine(GameRegistry.BuildingDatabase[buildingsToUnlock[i]].info.GetNameString());
		}
		return prod.ToString();
	}
}
