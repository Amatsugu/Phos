using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

using UnityEngine;

namespace Amatsugu.Phos.Weather
{
	[UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
	public class InitializeWeatherPrefabSystem : GameObjectConversionSystem
    {
		protected override void OnUpdate()
		{
			
		}
    }

	public class InitializeWeatherConvertClouds : GameObjectConversionSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((InitializeWeather weather) =>
			{
				var e = GetPrimaryEntity(weather.cloudPrefab);
				var w = GetPrimaryEntity(weather);
				DstEntityManager.AddComponentData(w, new WeatherData
				{
					cloud = e,
					clouldHeight = weather.clouldHeight,
					fieldWidth = weather.fieldWidth,
					fieldHeight = weather.fieldHeight
				});
			});
		}
	}

	public class InitializeCloudsSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			var innerR = HexCoords.CalculateInnerRadius(2);
			Entities.WithNone<Disabled>().ForEach((Entity e, ref WeatherData w) =>
			{
				//var clouds = new NativeArray<Entity>(w.fieldHeight * w.fieldWidth, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
				for (int z = 0; z < w.fieldHeight; z++)
				{
					for (int x = 0; x < w.fieldWidth; x++)
					{
						var pos = HexCoords.OffsetToWorldPosXZ(x, z, innerR, 2);
						pos.y = w.clouldHeight;
						var c = PostUpdateCommands.Instantiate(w.cloud);
						//PostUpdateCommands.AddComponent<CloudData>(e);
						PostUpdateCommands.SetComponent(c, new Translation { Value = pos });
						PostUpdateCommands.AddComponent(c, new NonUniformScale { Value = 4 });
						PostUpdateCommands.AddComponent(c, new CloudData { pos = pos, index = x + z * w.fieldWidth });
					}
				}
				PostUpdateCommands.AddComponent<Disabled>(e);
				//clouds.Dispose();
			});
		}
	}

	public struct WeatherData : IComponentData
	{
		public Entity cloud;
		public float clouldHeight;
		public int fieldHeight;
		public int fieldWidth;
	}
}
