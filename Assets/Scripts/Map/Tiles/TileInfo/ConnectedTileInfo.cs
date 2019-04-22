using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Connected Tile")]
public class ConnectedTileInfo : BuildingTileInfo
{
	public BuildingTileInfo[] tileConnections;
	public bool connectToSelf = false;
	public MeshEntityRotatable connectionMesh;

	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new ConnectedTile(pos, height, this);
	}
}
