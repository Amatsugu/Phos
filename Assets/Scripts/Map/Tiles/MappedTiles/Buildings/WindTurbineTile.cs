public class WindTurbileTile : TickedTile
{
	public WindTurbineInfo turbineInfo;

	public WindTurbileTile(HexCoords coords, float height, Map map, WindTurbineInfo tInfo) : base(coords, height, map, tInfo)
	{
		turbineInfo = tInfo;
	}

	public override void OnDisconnected()
	{
		base.OnDisconnected();

	}

	public override void OnConnected()
	{
		base.OnConnected();
	}
}