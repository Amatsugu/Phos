//TODO: Might not need this

using Amatsugu.Phos.TileEntities;

namespace Amatsugu.Phos.Tiles
{
	public class OffshoreBuildingTile : PoweredBuildingTile
	{
		public OffshoreBuildingTile(HexCoords coords, float height, Map map, BuildingTileEntity tInfo) : base(coords, height, map, tInfo)
		{
		}
	}
}