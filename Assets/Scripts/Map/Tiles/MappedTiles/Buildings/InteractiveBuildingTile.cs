namespace Amatsugu.Phos.Tiles
{
	public class InteractiveBuildingTile : PoweredBuildingTile
	{
		public InteractiveBuildingTileInfo uiInfo;

		public InteractiveBuildingTile(HexCoords coords, float height, Map map, InteractiveBuildingTileInfo tInfo) : base(coords, height, map, tInfo)
		{
			uiInfo = tInfo;
		}
	}
}