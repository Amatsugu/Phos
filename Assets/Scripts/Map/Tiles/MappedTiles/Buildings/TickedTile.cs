public class TickedBuildingTile : PoweredBuildingTile
{
	private bool _isTicking;

	public TickedBuildingTile(HexCoords coords, float height, Map map, BuildingTileEntity tInfo) : base(coords, height, map, tInfo)
	{
	}

	public override void OnConnected()
	{
		base.OnConnected();
		if (_isTicking)
			return;
		if (!IsBuilt)
			return;
		GameEvents.OnGameTick += OnTick;
		_isTicking = true;
	}

	protected override void OnBuilt()
	{
		base.OnBuilt();
		if (_isTicking)
			return;
		if (!HasHQConnection)
			return;
		GameEvents.OnGameTick += OnTick;
		_isTicking = true;
	}

	public override void OnDisconnected()
	{
		base.OnDisconnected();
		if (!_isTicking)
			return;
		GameEvents.OnGameTick -= OnTick;
		_isTicking = false;
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		if (!_isTicking)
			return;
		GameEvents.OnGameTick -= OnTick;
		_isTicking = false;
	}

	protected virtual void OnTick()
	{
	}
}