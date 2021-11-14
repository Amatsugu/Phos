using Amatsugu.Phos.Units;

using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

namespace Amatsugu.Phos
{
	public class UnitFactorySystem : ComponentSystem
	{
		private int _curId;
		private NativeList<QueuedUnit> _queue;
		private NativeList<PendingBuildOrder> _pendingBuildOrders;
		private UnitDatabase _unitDatabase;

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
			_curId = 0;

			_queue = new NativeList<QueuedUnit>(50, Allocator.Persistent);
			_pendingBuildOrders = new NativeList<PendingBuildOrder>(20, Allocator.Persistent);
			_unitDatabase = GameRegistry.UnitDatabase;
		}

		protected override void OnUpdate()
		{
			Entities.WithAllReadOnly<DynamicBuffer<GenericPrefab>>().ForEach((DynamicBuffer<GenericPrefab> prefabs) =>
			{
				for (int i = 0; i < _pendingBuildOrders.Length; i++)
				{
					var order = _pendingBuildOrders[i];
					var unit = _unitDatabase[order.unit];
					if(order.hasFactory)
					{
						if(!EntityManager.Exists(order.factory))
							continue;
						PostUpdateCommands.AddComponent<FactoryReadyTag>(order.factory);
					}
					unit.info.InstantiateUnit(order.pos, prefabs, PostUpdateCommands, order.faction);
				}

				_pendingBuildOrders.Clear();
			});

			Entities.WithAllReadOnly<Translation, HexPosition, UnitFactoryTag, FactoryReadyTag>().ForEach((Entity e, ref Translation pos, ref HexPosition coords) =>
			{
				int offset = 0;
				for (int i = 0; i < _queue.Length; i++)
				{
					var item = _queue[i - offset];
					if (item.factory != coords)
						continue;
					_pendingBuildOrders.Add(new PendingBuildOrder
					{
						faction = item.faction,
						factory = e,
						hasFactory = true,
						isCreated = true,
						pos = pos.Value,
						unit = item.unit,
					});
					_queue.RemoveAt(i + offset);
					offset++;
					PostUpdateCommands.RemoveComponent<FactoryReadyTag>(e);
				}
			});
		}

		/// <summary>
		/// Queue a new unit to be built from a specified factory
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="factory"></param>
		/// <param name="faction"></param>
		/// <returns></returns>
		public int QueueUnit(UnitIdentifier unit, HexCoords factory, Faction faction)
		{
			var id = _curId++;

			_queue.Add(new QueuedUnit
			{
				faction = faction,
				factory = factory,
				id = id,
				unit = unit,
				isCreated = true
			});

			return id;
		}

		public bool CancelItem(int id)
		{
			for (int i = 0; i < _queue.Length; i++)
			{
				QueuedUnit unit = _queue[i];
				if(unit.id == id)
				{
					_queue.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Instantly build a new unit
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="pos"></param>
		/// <param name="faction"></param>
		/// <returns></returns>
		public void BuildUnit(UnitIdentifier unit, float3 pos, Faction faction)
		{
			_pendingBuildOrders.Add(new PendingBuildOrder
			{
				isCreated = true,
				faction = faction,
				pos = pos,
				unit = unit
			});
		}

		private struct PendingBuildOrder
		{
			public Faction faction;
			public float3 pos;
			public UnitIdentifier unit;
			public bool isCreated;
			public bool hasFactory;
			public Entity factory;
		}

		private struct QueuedUnit
		{
			public int id;
			public UnitIdentifier unit;
			public Faction faction;
			public HexCoords factory;
			public bool isCreated;
		}
	}
}
