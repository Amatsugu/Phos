public class EnemyBuildingTile : BuildingTile
{
	public EnemyBuildingTile(HexCoords coords, float height, Map map, BuildingTileEntity tInfo) : base(coords, height, map, tInfo)
	{
		_isBuilt = true;
	}

	protected override void OnBuilt()
	{
		//base.OnBuilt();
	}
}