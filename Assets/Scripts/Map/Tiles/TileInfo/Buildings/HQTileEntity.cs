using Amatsugu.Phos.Tiles;
using Amatsugu.Phos.Units;

using System;

using UnityEngine;

namespace Amatsugu.Phos.TileEntities
{
	[CreateAssetMenu(menuName = "Map Asset/Tile/HQ")]
	public class HQTileEntity : BuildingTileEntity
	{
		public ResourceIndentifier[] startingResources;

		public SubHQTileEntity[] subHQTiles;
		public MobileUnitEntity unitInfo;
		public int unitCount = 16;

		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return new HQTile(pos, height, map, this);
		}

		public override BuildingTile CreateTile(Map map, HexCoords pos, float height, int rotation)
		{
			return CreateTile(map, pos, height) as BuildingTile;
		}


		private void OnValidate()
		{
			if (subHQTiles?.Length != 6)
				Array.Resize(ref subHQTiles, 6);
		}
	}
}