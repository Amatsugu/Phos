using System;

using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/HQ")]
public class HQTileInfo : BuildingTileEntity
{
	public ResourceIndentifier[] startingResources;

	public SubHQTileInfo[] subHQTiles;
	public MobileUnitEntity unitInfo;

	public override Tile CreateTile(Map map, HexCoords pos, float height)
	{
		return new HQTile(pos, height, map, this);
	}

	private void OnValidate()
	{
		if (subHQTiles?.Length != 6)
			Array.Resize(ref subHQTiles, 6);
	}
}