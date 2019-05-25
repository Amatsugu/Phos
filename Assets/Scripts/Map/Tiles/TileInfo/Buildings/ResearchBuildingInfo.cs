using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ResearchBuildingInfo : BuildingTileInfo
{
	public override IEnumerable<ComponentType> GetComponents()
	{
		return base.GetComponents().Append(typeof(ResearchBuildingTag));
	}

	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new ResearchBuildingTile(pos, height, this);
	}

}
