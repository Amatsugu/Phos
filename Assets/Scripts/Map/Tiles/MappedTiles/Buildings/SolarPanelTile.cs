public class SolarPanelTile : TickedTile
{
	public SolarPanelTileInfo solarInfo;

	public SolarPanelTile(HexCoords coords, float height, SolarPanelTileInfo tInfo) : base(coords, height, tInfo)
	{
		solarInfo = tInfo;
	}
}