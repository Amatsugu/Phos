using Amatsugu.Phos.Tiles;

using UnityEngine;

namespace Amatsugu.Phos.TileEntities
{
	[CreateAssetMenu(menuName = "Map Asset/Tile/Resource Tile Info")]
	public class ResourceTileInfo : TileEntity
	{
		public ResourceIndentifier[] resourceYields;

		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return new ResourceTile(pos, height, map, this);
		}
	}
}