using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Amatsugu.Phos.Tiles
{
	public class MetaTile : PoweredBuildingTile
	{
		public PoweredBuildingTile parentTile;

		public MetaTile(HexCoords coords, float height, Map map, MetaTileEntity tInfo) : base(coords, height, map, tInfo)
		{

		}

		public override void TileUpdated(Tile src, TileUpdateType updateType)
		{
			if (src.Coords == parentTile.Coords)
			{
				if (updateType == TileUpdateType.Removed)
					map.RevertTile(this);
				return;
			}
			parentTile.TileUpdated(src, updateType);
		}

		public override void HQConnected()
		{
			parentTile.HQConnected();
		}

		public override void HQDisconnected()
		{
			parentTile.HQDisconnected();
		}
	}
}