using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Sub HQ")]
public class SubHQTileInfo : BuildingTileInfo
{
	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new SubHQTile(pos, height, this);
	}
}