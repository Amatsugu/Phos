using Unity.Entities;
using Unity.Mathematics;

public class WindTurbileTile : TickedBuildingTile
{
	public WindTurbineTileEntity turbineInfo;

	private WeatherSystem _weatherSystem;

	public WindTurbileTile(HexCoords coords, float height, Map map, WindTurbineTileEntity tInfo) : base(coords, height, map, tInfo)
	{
		turbineInfo = tInfo;
		_weatherSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<WeatherSystem>();
	}

	protected override void OnTick()
	{
		base.OnTick();
		var windSpeed = math.length(_weatherSystem.windDir);
		var eff = windSpeed.Remap(0, 10, turbineInfo.efficencyRange.x, turbineInfo.efficencyRange.y);
		var building = GetBuildingEntity();
		Map.EM.SetComponentData(building, new ProductionMulti { Value = prodMulti * eff });
	}

}