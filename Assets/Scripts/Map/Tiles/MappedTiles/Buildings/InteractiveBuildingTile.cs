using Amatsugu.Phos.TileEntities;

namespace Amatsugu.Phos.Tiles
{
	public class InteractiveBuildingTile : PoweredBuildingTile
	{
		public InteractiveBuildingTileInfo uiInfo;

		public InteractiveBuildingTile(HexCoords coords, float height, Map map, InteractiveBuildingTileInfo tInfo, int rotation) : base(coords, height, map, tInfo, rotation)
		{
			uiInfo = tInfo;
		}
	}
}