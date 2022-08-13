using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Amatsugu.Phos
{
	[BurstCompatible]
	public class IndicatorSystem : ComponentSystem
	{
		private NativeList<Indicator> _indicatorsToAdd;
		private int _mapWidth;

		protected override void OnCreate()
		{
			GameRegistry.INST.indicatorSystem = this;
			base.OnCreate();
			GameEvents.OnMapLoaded += Init;
			GameEvents.OnMapDestroyed += DeInit;
		}

		private void Init()
		{
			_mapWidth = GameRegistry.GameMap.totalWidth;
			_indicatorsToAdd = new NativeList<Indicator>(GameRegistry.GameMap.tileCount / 6, Allocator.Persistent);
			
		}

		private void DeInit()
		{
			if (_indicatorsToAdd.IsCreated)
				_indicatorsToAdd.Dispose();
		}

		protected override void OnUpdate()
		{
			Entities.WithAllReadOnly<GenericPrefab>().ForEach((DynamicBuffer<GenericPrefab> prefabBuffer) =>
			{
				for (int i = 0; i < _indicatorsToAdd.Length; i++)
				{
					var indicator = _indicatorsToAdd[i];
					var prefab = prefabBuffer[indicator.prefabId];
					var entity = PostUpdateCommands.Instantiate(prefab.value);
					PostUpdateCommands.SetComponent(entity, new Translation()
					{
						Value = indicator.pos
					});
					PostUpdateCommands.SetComponent(entity, new Rotation()
					{
						Value = indicator.rotation
					});

					PostUpdateCommands.AddComponent(entity, new NonUniformScale()
					{
						Value = indicator.scale
					});
					PostUpdateCommands.AddComponent(entity, new DeathTime
					{
						Value = 0
					});
				}
			});

			_indicatorsToAdd.Clear();
		}

		public void SetIndicator(HexCoords coords, float height, int prefabId)
		{

			var pos = coords.WorldPos;
			pos.y = height;
			_indicatorsToAdd.Add(new Indicator
			{
				pos = pos,
				prefabId = prefabId,
				rotation = quaternion.identity,
				scale = 0.9f
			});
		}

		public void SetIndicator(float3 pos, quaternion rot, float3 scale, int prefab)
		{
			_indicatorsToAdd.Add(new Indicator
			{
				pos = pos,
				scale = scale,
				rotation = rot,
				prefabId = prefab
			});
		}

		protected override void OnDestroy()
		{
			DeInit();
			base.OnDestroy();
		}

		private struct Indicator
		{
			public float3 pos;
			public quaternion rotation;
			public float3 scale;
			public int prefabId;
		}
	}

	public struct IndicatorIdentifier : IComponentData
	{
		public int Value;
	}
}