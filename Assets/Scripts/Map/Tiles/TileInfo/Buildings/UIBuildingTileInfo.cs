using Amatsugu.Phos.Tiles;

using UnityEngine;

namespace Amatsugu.Phos.TileEntities
{
	public class InteractiveBuildingTileInfo : BuildingTileEntity
	{
		public RectTransform UIScreen;

		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return new InteractiveBuildingTile(pos, height, map, this);
		}
	}
}