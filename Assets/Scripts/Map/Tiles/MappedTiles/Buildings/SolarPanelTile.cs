public class SolarPanelTile : TickedTile
{
	public SolarPanelTileInfo solarInfo;

	public SolarPanelTile(HexCoords coords, float height, Map map, SolarPanelTileInfo tInfo) : base(coords, height, map, tInfo)
	{
		solarInfo = tInfo;
	}
}