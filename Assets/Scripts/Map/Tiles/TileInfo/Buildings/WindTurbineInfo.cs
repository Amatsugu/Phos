public class WindTurbineInfo : BuildingTileEntity
{
	public override Tile CreateTile(Map map, HexCoords pos, float height)
	{
		return new WindTurbileTile(pos, height, map, this);
	}
}