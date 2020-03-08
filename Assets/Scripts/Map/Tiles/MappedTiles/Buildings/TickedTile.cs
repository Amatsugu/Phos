public class TickedTile : PoweredBuildingTile
{
	public TickedTile(HexCoords coords, float height, BuildingTileInfo tInfo) : base(coords, height, tInfo)
	{
	}

	protected override void OnBuilt()
	{
		base.OnBuilt();
		EventManager.AddEventListener("OnTick", OnTick);
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		EventManager.RemoveEventListener("OnTick", OnTick);
	}

	protected virtual void OnTick()
	{
	}
}