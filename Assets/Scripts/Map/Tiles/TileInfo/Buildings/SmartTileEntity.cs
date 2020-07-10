
using Amatsugu.Phos.Tiles;

using UnityEngine;

namespace Amatsugu.Phos.TileEntities
{
	[CreateAssetMenu(menuName = "Map Asset/Tile/Connected Tile")]
	public class SmartTileEntity : TileEntity
	{
		public MeshEntityRotatable connectionMesh;
		public MeshEntityRotatable vertexMesh;
		public bool alwaysShowVertex;
		public bool connectToSelf = false;

		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return new SmartTile(pos, height, map, this);
		}
	}
}