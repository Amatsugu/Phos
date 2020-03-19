using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Resource Conduit")]
public class ResourceConduitTileInfo : BuildingTileEntity
{
	public int poweredRange;
	public int connectionRange;
	public MeshEntityRotatable lineEntity;
	public MeshEntityRotatable lineEntityInactive;
	public MeshEntityRotatable energyPacket;
	public float powerLineOffset;

	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new ResourceConduitTile(pos, height, this);
	}
}