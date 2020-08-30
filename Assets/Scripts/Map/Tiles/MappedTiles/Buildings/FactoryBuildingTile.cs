using Amatsugu.Phos.TileEntities;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
	public class FactoryBuildingTile : PoweredBuildingTile
	{
		public FactoryTileEntity factoryInfo;
		public FactoryBuildingTile(HexCoords coords, float height, Map map, FactoryTileEntity tInfo) : base(coords, height, map, tInfo)
		{
			factoryInfo = tInfo;
		}

	}
}