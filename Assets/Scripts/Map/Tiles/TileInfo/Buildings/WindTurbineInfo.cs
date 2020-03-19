public class WindTurbineInfo : BuildingTileEntity
{
	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new WindTurbileTile(pos, height, this);
	}
}