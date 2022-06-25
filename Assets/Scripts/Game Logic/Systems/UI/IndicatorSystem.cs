using System.Collections;
using System.Collections.Generic;
using System.Text;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

namespace Amatsugu.Phos
{
	[BurstCompatible]
	public class IndicatorSystem : ComponentSystem
	{
		private NativeArray<int> _indicatorArray;
		private NativeList<Indicator> _indicatorsToAdd;
		private NativeParallelHashSet<int> _indicatorsToRemove;
		private int _mapWidth;
		private int _nextId;
		private bool _clearAll;
		private EntityQuery _indicatorQuery;

		protected override void OnCreate()
		{
			GameRegistry.INST.indicatorSystem = this;
			base.OnCreate();
			GameEvents.OnMapLoaded += Init;
			GameEvents.OnMapDestroyed += DeInit;

			var indicatorDesc = new EntityQueryDesc
			{
				All = new ComponentType[]
				{
					ComponentType.ReadOnly<IndicatorIdentifier>(),
				}
			};

			_indicatorQuery = GetEntityQuery(indicatorDesc);
		}

		private void Init()
		{
			_mapWidth = GameRegistry.GameMap.totalWidth;
			_indicatorArray = new NativeArray<int>(GameRegistry.GameMap.tileCount, Allocator.Persistent);
			_indicatorsToAdd = new NativeList<Indicator>(_indicatorArray.Length / 6, Allocator.Persistent);
			_indicatorsToRemove = new NativeParallelHashSet<int>(_indicatorArray.Length / 6, Allocator.Persistent);
			_nextId = 0;
			for (int i = 0; i < _indicatorArray.Length; i++)
			{
				_indicatorArray[i] = -1;
			}
		}

		private void DeInit()
		{
			if(_indicatorArray.IsCreated)
				_indicatorArray.Dispose();
			if (_indicatorsToAdd.IsCreated)
				_indicatorsToAdd.Dispose();
			if (_indicatorsToRemove.IsCreated)
				_indicatorsToRemove.Dispose();
		}

		protected override void OnUpdate()
		{
			if (_clearAll)
				PostUpdateCommands.DestroyEntitiesForEntityQuery(_indicatorQuery);
			Entities.WithAllReadOnly<MapTag>().ForEach((DynamicBuffer<GenericPrefab> prefabBuffer) =>
			{
				for (int i = 0; i < _indicatorsToAdd.Length; i++)
				{
					_nextId++;
					var indicator = _indicatorsToAdd[i];
					var prefab = prefabBuffer[indicator.prefabId];
					var entity = PostUpdateCommands.Instantiate(prefab.value);
					var curId = _nextId;
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
					PostUpdateCommands.AddComponent(entity, new IndicatorIdentifier
					{
						Value = curId
					});

					if(indicator.index < _indicatorArray.Length)	
						_indicatorArray[indicator.index] = curId;
				}
			});



			_clearAll = false;


			_indicatorsToAdd.Clear();
			_indicatorsToRemove.Clear();
		}

		public void SetIndicator(HexCoords coords, float height, int prefabId)
		{
			var index = coords.ToIndex(_mapWidth);
			if (_indicatorArray[index] != -1)
				_indicatorsToRemove.Add(_indicatorArray[index]);

			var pos = coords.WorldPos;
			pos.y = height;
			_indicatorsToAdd.Add(new Indicator
			{
				index = index,
				pos = pos,
				prefabId = prefabId,
				rotation = quaternion.identity,
				scale = 0.9f
			});
		}

		public void UnsetAllIndicators()
		{
			_clearAll = true;
			_nextId = 0;
		}

		protected override void OnDestroy()
		{
			DeInit();
			base.OnDestroy();
		}

		private struct Indicator
		{
			public int index;
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
