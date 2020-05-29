using Amatsugu.Phos.Tiles;

using UnityEngine;

namespace Amatsugu.Phos.TileEntities
{
	[CreateAssetMenu(menuName = "Map Asset/Tile/Sub HQ")]
	public class SubHQTileEntity : BuildingTileEntity
	{
		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return new SubHQTile(pos, height, map, this);
		}
	}
}