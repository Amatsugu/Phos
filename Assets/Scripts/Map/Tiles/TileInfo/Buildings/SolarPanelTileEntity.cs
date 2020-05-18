using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Building/Solar Panel")]
public class SolarPanelTileEntity : BuildingTileEntity
{
	public override Tile CreateTile(Map map, HexCoords pos, float height)
	{
		return new SolarPanelTile(pos, height, map, this);
	}
}