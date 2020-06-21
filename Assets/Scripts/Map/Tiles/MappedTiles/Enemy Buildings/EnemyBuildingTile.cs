﻿using Amatsugu.Phos.TileEntities;

namespace Amatsugu.Phos.Tiles
{
	public class EnemyBuildingTile : BuildingTile
	{
		public EnemyBuildingTile(HexCoords coords, float height, Map map, BuildingTileEntity tInfo) : base(coords, height, map, tInfo)
		{
			isBuilt = true;
		}

		protected override void OnBuilt()
		{
			//base.OnBuilt();
		}

	}
}