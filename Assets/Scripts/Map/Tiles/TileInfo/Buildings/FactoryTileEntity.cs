using Amatsugu.Phos.Tiles;

using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;

namespace Amatsugu.Phos.TileEntities
{
    public class FactoryTileEntity : BuildingTileEntity
    {
		[Header("Factory")]
        public UnitIdentifier[] unitsToBuild;

		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return CreateTile(map, pos, height, 0);
		}

		public override Tile CreateTile(Map map, HexCoords pos, float height, int rotation)
		{
			return new FactoryBuildingTile(pos, height, map, this, rotation);
		}
	}
}
