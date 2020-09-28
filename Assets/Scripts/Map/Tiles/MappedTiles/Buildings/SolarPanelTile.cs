using Amatsugu.Phos;
using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;

public class SolarPanelTile : TickedBuildingTile
{
	public SolarPanelTileEntity solarInfo;

	public SolarPanelTile(HexCoords coords, float height, Map map, SolarPanelTileEntity tInfo, int rotation) : base(coords, height, map, tInfo, rotation)
	{
		solarInfo = tInfo;
	}
}