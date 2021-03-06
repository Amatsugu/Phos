﻿using Amatsugu.Phos.Tiles;

using UnityEngine;

namespace Amatsugu.Phos.TileEntities
{
	[CreateAssetMenu(menuName = "Map Asset/Tile/Resource Conduit")]
	public class ResourceConduitTileEntity : BuildingTileEntity
	{
		public int poweredRange;
		public int connectionRange;
		public MeshEntityRotatable lineEntity;
		public MeshEntityRotatable lineEntityInactive;
		public MeshEntityRotatable energyPacket;
		public float powerLineOffset;

		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return CreateTile(map, pos, height, 0);
		}

		public override Tile CreateTile(Map map, HexCoords pos, float height, int rotation)
		{
			return new ResourceConduitTile(pos, height, map, this, rotation);
		}
	}
}