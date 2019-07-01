public class ResourceConduitTileInfo : BuildingTileInfo
{
	public int connectionRange;

	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new ResourceConduitTile(pos, height, this);
	}
}