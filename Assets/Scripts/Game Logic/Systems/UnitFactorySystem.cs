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

		protected override void OnCreate()
		{
			_curId = 0;
			_queue = new NativeList<QueuedUnit>(50, Allocator.Persistent);
			_pendingBuildOrders = new NativeList<PendingBuildOrder>(20, Allocator.Persistent);
			base.OnCreate();
		}

		protected override void OnStartRunning()
		{

			_unitDatabase = GameRegistry.UnitDatabase;
			base.OnStartRunning();
		}

		protected override void OnUpdate()
		{
			Entities.ForEach((DynamicBuffer<GenericPrefab> prefabs) =>
			{
				for (int i = 0; i < _pendingBuildOrders.Length; i++)
				{
					var order = _pendingBuildOrders[i];
					if (!order.isCreated)
						continue;
					var unit = _unitDatabase[order.unit];
					if(order.hasFactory)
					{
						if(!EntityManager.Exists(order.factory))
							continue;
						PostUpdateCommands.AddComponent<FactoryReadyTag>(order.factory);
					}
					if(!order.isBuilding)
					{
						order.isBuilding = true;
						order.buildCompleteTime += Time.ElapsedTime;
						_pendingBuildOrders[i] = order;
						//TODO: Create construction animation
						continue;
					}
					if(order.buildCompleteTime <= Time.ElapsedTime)
					{
						_pendingBuildOrders.RemoveAtSwapBack(i);
						i--;
						unit.info.InstantiateUnit(order.pos, prefabs, PostUpdateCommands, order.faction);
					}
				}

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
						buildCompleteTime = item.buildTime
					});
					_queue.RemoveAt(i + offset);
					offset++;
					PostUpdateCommands.RemoveComponent<FactoryReadyTag>(e);
				}
			});
		}


		protected override void OnDestroy()
		{
			_pendingBuildOrders.Dispose();
			_queue.Dispose();
			base.OnDestroy();
		}
		/// <summary>
		/// Queue a new unit to be built from a specified factory
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="factory"></param>
		/// <param name="faction"></param>
		/// <param name="buildTime"></param>
		/// <returns></returns>
		public int QueueUnit(UnitIdentifier unit, HexCoords factory, Faction faction, double buildTime)
		{
			var id = _curId++;

			_queue.Add(new QueuedUnit
			{
				faction = faction,
				factory = factory,
				id = id,
				unit = unit,
				isCreated = true,
				buildTime = buildTime,
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
		public void BuildUnit(UnitIdentifier unit, float3 pos, Faction faction, double buildTime = 0)
		{
			_pendingBuildOrders.Add(new PendingBuildOrder
			{
				isCreated = true,
				faction = faction,
				pos = pos,
				unit = unit,
				isBuilding = buildTime == 0,
				buildCompleteTime = Time.ElapsedTime + buildTime
			});
		}

		private struct PendingBuildOrder
		{
			public Faction faction;
			public float3 pos;
			public int unit;
			public bool isCreated;
			public bool hasFactory;
			public Entity factory;
			public bool isBuilding;
			public double buildCompleteTime;
		}

		private struct QueuedUnit
		{
			public int id;
			public int unit;
			public Faction faction;
			public HexCoords factory;
			public bool isCreated;
			public double buildTime;
		}
	}
}
