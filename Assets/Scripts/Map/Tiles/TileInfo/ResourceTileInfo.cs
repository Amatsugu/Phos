using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Resource Tile Info")]
public class ResourceTileInfo : TileEntity
{
	public ResourceIndentifier[] resourceYields;

	public override Tile CreateTile(Map map, HexCoords pos, float height)
	{
		return new ResourceTile(pos, height, map, this);
	}
}