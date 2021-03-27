
using Amatsugu.Phos.TileEntities;

using AnimationSystem.Animations;

using System;

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


		public WindTurbileTile(HexCoords coords, float height, Map map, WindTurbineTileEntity tInfo, int rotation) : base(coords, height, map, tInfo, rotation)
		{
			turbineInfo = tInfo;
			_weatherSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<WeatherSystem>();
		}

		protected override void OnTick()
		{
			base.OnTick();
			UpdateWind();
		}

		[Obsolete]
		void UpdateWind()
		{
			//TODO: New method of handling ticked tiles
			//var windSpeed = math.length(_weatherSystem.WindSpeed);
			//float eff;
			//if (windSpeed == 0)
			//	eff = 0;
			//else
			//	eff = windSpeed.Remap(0, .5f, turbineInfo.efficencyRange.x, turbineInfo.efficencyRange.y);
			//var building = GetBuildingEntity();
			//GameRegistry.EntityManager.SetComponentData(building, new ProductionMulti { Value = totalBuffs.productionMulti * eff });
			//Map.EM.SetComponentData(_blade, new RotateSpeed { Value = turbineInfo.maxSpinSpeed * eff });
		}

		protected override void ApplyBuffs()
		{
			base.ApplyBuffs();
			UpdateWind();
		}

		public override void OnDisconnected()
		{
			base.OnDisconnected();
			//Map.EM.SetComponentData(_blade, new RotateSpeed { Value = 0 });
		}

	}
}