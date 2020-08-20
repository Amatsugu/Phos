using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amatsugu.Phos.TileEntities
{
	[CreateAssetMenu(menuName = "Map Asset/Tile/Meta Tile")]
	public class MetaTileEntity : BuildingTileEntity
	{
		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			throw new NotImplementedException();
			//return new MetaTile(pos, height, map, this);
		}
	}
}