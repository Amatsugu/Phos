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
		private NativeList<PendingUnitBuildOrder> _pendingBuildOrders;
		private Map _map;
		private UnitDatabase _unitDatabase;

		protected override void OnCreate()
		{
			_curId = 0;
			_queue = new NativeList<QueuedUnit>(50, Allocator.Persistent);
			_pendingBuildOrders = new NativeList<PendingUnitBuildOrder>(20, Allocator.Persistent);
			base.OnCreate();
		}

		protected override void OnStartRunning()
		{
			_unitDatabase = GameRegistry.UnitDatabase;
			_map = GameRegistry.GameMap;
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
					if (order.isCanceled)
					{
						if (order.isBuilding)
						{
							PostUpdateCommands.AddComponent<FactoryReadyTag>(order.factory);
							//TODO: Remove construction animation
						}
						_pendingBuildOrders.RemoveAtSwapBack(i);
						i--;
						continue;
					}
					//Build Animation
					if (!order.isBuilding)
					{
						order.isBuilding = true;
						order.buildCompleteTime = Time.ElapsedTime + order.buildTime;
						_pendingBuildOrders[i] = order;
						GameEvents.InvokeOnUnitConstructionStart(order);
						if(order.hasFactory)
							PostUpdateCommands.RemoveComponent<FactoryReadyTag>(order.factory);
						//TODO: Create construction animation
						continue;
					}
					//Finish Build
					if (order.buildCompleteTime <= Time.ElapsedTime)
					{
						GameEvents.InvokeOnUnitConstructionEnd(order.id);
						if(order.hasFactory)
							PostUpdateCommands.AddComponent<FactoryReadyTag>(order.factory);
						_pendingBuildOrders.RemoveAtSwapBack(i);
						i--;
						unit.info.InstantiateUnit(order.pos, prefabs, PostUpdateCommands, order.faction);
					}
				}
			});

			Entities.WithAllReadOnly<HexPosition, UnitFactoryTag, FactoryReadyTag>().ForEach((Entity e, ref HexPosition coords) =>
			{
				int offset = 0;
				for (int i = 0; i < _queue.Length; i++)
				{
					var item = _queue[i - offset];
					if (item.factory != coords)
						continue;
					_pendingBuildOrders.Add(new PendingUnitBuildOrder
					{
						id = item.id,
						faction = item.faction,
						factory = e,
						hasFactory = true,
						factoryCoords = item.factory,
						isCreated = true,
						pos = _map[coords].SurfacePoint,
						unit = item.unit,
						buildTime = item.buildTime,
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

			var order = new QueuedUnit
			{
				faction = faction,
				factory = factory,
				id = id,
				unit = unit,
				isCreated = true,
				buildTime = buildTime,
			};

			_queue.Add(order);

			GameEvents.InvokeOnUnitQueued(order);

			return id;
		}

		public bool CancelOrder(int id)
		{
			for (int i = 0; i < _queue.Length; i++)
			{
				QueuedUnit unit = _queue[i];
				if (unit.id == id)
				{
					GameEvents.InvokeOnUnitDequeued(id);
					_queue.RemoveAt(i);
					return true;
				}
			}

			for (int i = 0; i < _pendingBuildOrders.Length; i++)
			{
				var order = _pendingBuildOrders[i];
				if(order.id == id)
				{
					order.isCanceled = true;
					_pendingBuildOrders[i] = order;
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
			_pendingBuildOrders.Add(new PendingUnitBuildOrder
			{
				id = _curId++,
				isCreated = true,
				faction = faction,
				pos = pos,
				unit = unit,
				isBuilding = buildTime == 0,
				buildCompleteTime = Time.ElapsedTime + buildTime,
				buildTime = buildTime
			});
		}
	}

	public struct PendingUnitBuildOrder
	{
		public int id;
		public Faction faction;
		public float3 pos;
		public int unit;
		public bool isCreated;
		public bool hasFactory;
		public Entity factory;
		public HexCoords factoryCoords;
		public bool isBuilding;
		public double buildCompleteTime;
		public double buildTime;
		public bool isCanceled;
	}

	public struct QueuedUnit
	{
		public int id;
		public int unit;
		public Faction faction;
		public HexCoords factory;
		public bool isCreated;
		public double buildTime;
	}
}