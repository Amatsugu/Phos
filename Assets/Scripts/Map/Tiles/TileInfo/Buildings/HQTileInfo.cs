using System;

using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/HQ")]
public class HQTileInfo : BuildingTileInfo
{
	public ResourceIndentifier[] startingResources;

	public SubHQTileInfo[] subHQTiles;
	public MobileUnitInfo unitInfo;

	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new HQTile(pos, height, this);
	}

	private void OnValidate()
	{
		if (subHQTiles?.Length != 6)
			Array.Resize(ref subHQTiles, 6);
	}
}