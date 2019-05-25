using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Building/Resoruce Gathering")]
public class ResourceGatheringBuildingInfo : BuildingTileInfo
{
	public ResourceIndentifier[] resourcesToGather;
	public int gatherRange = 3;

	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new GatheringBuildingTile(pos, height, this);
	}
}
