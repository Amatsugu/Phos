using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Tech Building")]
public class TechBuildingEntity : BuildingTileEntity
{
	[Header("Tech Up")]
	public BuildingIdentifier[] buildingsToUnlock;

	public override Tile CreateTile(Map map, HexCoords pos, float height)
	{
		return base.CreateTile(map, pos, height);
	}
}
