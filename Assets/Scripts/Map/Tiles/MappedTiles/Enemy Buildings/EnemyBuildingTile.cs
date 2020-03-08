public class EnemyBuildingTile : BuildingTile
{
	public EnemyBuildingTile(HexCoords coords, float height, BuildingTileInfo tInfo) : base(coords, height, tInfo)
	{
		_isBuilt = true;
	}

	protected override void OnBuilt()
	{
		//base.OnBuilt();
	}
}