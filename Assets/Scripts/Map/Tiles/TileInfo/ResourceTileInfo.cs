using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Resource Tile Info")]
public class ResourceTileInfo : TileInfo
{
	public ResourceIndentifier[] resourceYields;

	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new ResourceTile(pos, height, this);
	}
}