using AnimationSystem.Animations;
using Unity.Entities;
using Unity.Mathematics;

public class WindTurbileTile : TickedBuildingTile
{
	public WindTurbineTileEntity turbineInfo;

	private WeatherSystem _weatherSystem;
	private Entity _blade;


	public WindTurbileTile(HexCoords coords, float height, Map map, WindTurbineTileEntity tInfo) : base(coords, height, map, tInfo)
	{
		turbineInfo = tInfo;
		_weatherSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<WeatherSystem>();
	}

	protected override void OnBuilt()
	{
		base.OnBuilt();
		_blade = turbineInfo.turbineBlade.Instantiate(SurfacePoint);
		Map.EM.AddComponentData(_blade, new RotateAxis { Value = math.up() });
		Map.EM.AddComponentData(_blade, new RotateSpeed { Value = 0 });
	}

	protected override void OnTick()
	{
		base.OnTick();
		var windSpeed = math.length(_weatherSystem.windDir);
		var eff = windSpeed.Remap(0, 20, turbineInfo.efficencyRange.x, turbineInfo.efficencyRange.y);
		var building = GetBuildingEntity();
		Map.EM.SetComponentData(building, new ProductionMulti { Value = buffs.productionMulti * eff });
		Map.EM.SetComponentData(_blade, new RotateSpeed { Value = turbineInfo.maxSpinSpeed * eff });
	}

	public override void OnDisconnected()
	{
		base.OnDisconnected();
		Map.EM.SetComponentData(_blade, new RotateSpeed { Value = 0 });
	}

	public override void Destroy()
	{
		base.Destroy();
		Map.EM.DestroyEntity(_blade);
	}

}