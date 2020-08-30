using Amatsugu.Phos.Tiles;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amatsugu.Phos.TileEntities
{
    public class FactoryTileEntity : BuildingTileEntity
    {
		[Header("Factory")]
        public UnitIdentifier[] unitsToBuild;

		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return new FactoryBuildingTile(pos, height, map, this);
		}
	}
}
