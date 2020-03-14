public class TickedTile : PoweredBuildingTile
{
	public TickedTile(HexCoords coords, float height, BuildingTileInfo tInfo) : base(coords, height, tInfo)
	{
	}

	protected override void OnBuilt()
	{
		base.OnBuilt();
		EventManager.AddEventListener(GameEvent.OnGameTick, OnTick);
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		EventManager.RemoveEventListener(GameEvent.OnGameTick, OnTick);
	}

	protected virtual void OnTick()
	{
	}
}