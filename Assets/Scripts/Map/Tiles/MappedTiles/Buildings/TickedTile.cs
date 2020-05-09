public class TickedTile : PoweredBuildingTile
{
	public TickedTile(HexCoords coords, float height, Map map, BuildingTileEntity tInfo) : base(coords, height, map, tInfo)
	{
	}

	protected override void OnBuilt()
	{
		base.OnBuilt();
		GameEvents.OnGameTick += OnTick;
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		GameEvents.OnGameTick -= OnTick;
	}

	protected virtual void OnTick()
	{
	}
}