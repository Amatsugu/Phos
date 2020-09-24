
using Amatsugu.Phos.Tiles;

using UnityEngine;

namespace Amatsugu.Phos.TileEntities
{
	[CreateAssetMenu(menuName = "Map Asset/Tile/Connected Tile")]
	public class SmartTileEntity : BuildingTileEntity
	{
		[Header("Wall")]
		public MeshEntityRotatable connectionMesh;
		public MeshEntityRotatable vertexMesh;
		public BuildingIdentifier[] connectTo;
		public bool connectToSelf = false;
		public bool alwaysShowVertex;

		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return new SmartTile(pos, height, map, this, 0);
		}

		
	}
}