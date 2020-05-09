using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Connected Tile")]
public class ConnectedTileInfo : BuildingTileEntity
{
	public BuildingTileEntity[] tileConnections;
	public bool connectToSelf = false;
	public MeshEntityRotatable connectionMesh;

	public override Tile CreateTile(Map map, HexCoords pos, float height)
	{
		return new ConnectedTile(pos, height, map, this);
	}
}