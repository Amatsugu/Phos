public class TickedTile : PoweredBuildingTile
{
	public TickedTile(HexCoords coords, float height, BuildingTileEntity tInfo) : base(coords, height, tInfo)
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