public class ResourceTile : Tile
{
	public readonly ResourceTileInfo resInfo;
	public HexCoords gatherer;

	public ResourceTile(HexCoords coords, float height, Map map, ResourceTileInfo tInfo) : base(coords, height, map, tInfo)
	{
		resInfo = tInfo;
	}

	public override TileEntity GetMeshEntity()
	{
		return originalTile ?? info;
	}
}