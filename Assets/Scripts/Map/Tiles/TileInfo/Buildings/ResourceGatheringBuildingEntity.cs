using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Building/Resoruce Gathering")]
public class ResourceGatheringBuildingEntity : BuildingTileEntity
{
	[Header("Gathering")]
	public ResourceIndentifier[] resourcesToGather;
	public int gatherRange = 3;

	public override Tile CreateTile(Map map, HexCoords pos, float height)
	{
		return new GatheringBuildingTile(pos, height, map, this);
	}

	public override string GetProductionString()
	{
		var gatherString = "";
		for (int i = 0; i < resourcesToGather.Length; i++)
		{
			var curRes = resourcesToGather[i];
			gatherString += $"+{ResourceDatabase.GetResourceString(curRes.id)} {curRes.ammount}/tile";
			if (i != resourcesToGather.Length - 1)
				gatherString += "\n";
		}
		var b = base.GetProductionString();
		if (b.Length > 0)
			b += "\n";
		return b + gatherString;
	}
}