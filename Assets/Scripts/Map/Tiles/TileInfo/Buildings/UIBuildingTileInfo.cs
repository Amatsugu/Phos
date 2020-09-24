using Amatsugu.Phos.Tiles;

using UnityEngine;

namespace Amatsugu.Phos.TileEntities
{
	public class InteractiveBuildingTileInfo : BuildingTileEntity
	{
		public RectTransform UIScreen;

		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return CreateTile(map, pos, height, 0);
		}

		public override Tile CreateTile(Map map, HexCoords pos, float height, int rotation)
		{
			return new InteractiveBuildingTile(pos, height, map, this, rotation);
		}
	}
}