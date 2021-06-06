
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
		}

		public override void OnDisconnected()
		{
			base.OnDisconnected();
			//Map.EM.SetComponentData(_blade, new RotateSpeed { Value = 0 });
		}

	}
}