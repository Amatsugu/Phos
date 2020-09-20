
using Amatsugu.Phos.TileEntities;

using AnimationSystem.Animations;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
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

		public override void RenderSubMeshes(quaternion rot)
		{
			base.RenderSubMeshes(rot);
			_blade = subMeshes[turbineInfo.turbineBladeSubMesh.id];
			Map.EM.AddComponentData(_blade, new RotateAxis { Value = math.up() });
			Map.EM.AddComponentData(_blade, new RotateSpeed { Value = 0 });
		}

		protected override void OnTick()
		{
			base.OnTick();
			UpdateWind();
		}

		void UpdateWind()
		{
			var windSpeed = math.length(_weatherSystem.WindSpeed);
			float eff;
			if (windSpeed == 0)
				eff = 0;
			else
				eff = windSpeed.Remap(0, .5f, turbineInfo.efficencyRange.x, turbineInfo.efficencyRange.y);
			var building = GetBuildingEntity();
			Map.EM.SetComponentData(building, new ProductionMulti { Value = totalBuffs.productionMulti * eff });
			Map.EM.SetComponentData(_blade, new RotateSpeed { Value = turbineInfo.maxSpinSpeed * eff });
		}

		protected override void ApplyBuffs()
		{
			base.ApplyBuffs();
			UpdateWind();
		}

		public override void OnDisconnected()
		{
			base.OnDisconnected();
			Map.EM.SetComponentData(_blade, new RotateSpeed { Value = 0 });
		}

	}
}