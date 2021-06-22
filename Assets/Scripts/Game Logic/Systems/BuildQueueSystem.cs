using Amatsugu.Phos;
using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;

using System;
using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;

public class BuildQueueSystem : ComponentSystem
{
	private Dictionary<int, BuildOrder> _pendingBuildOrders;
	private List<int> _readyToBuildOrders;
	private List<ConstructionOrder> _constructionOrders;
	private List<int> _removal;
	private int _curOrderID = 0;
	private Dictionary<HexCoords, bool> _factoryReady;

	private static BuildQueueSystem _INST;
	private bool _isReady;

	protected override void OnCreate()
	{
		GameEvents.OnMapLoaded += InitBuildQueue;
		_factoryReady = new Dictionary<HexCoords, bool>();
		GameEvents.OnUnitBuilt += OnUnitBuilt;
		_INST = this;
		GameRegistry.INST.buildQueueSystem = this;
	}

	private void OnUnitBuilt(HexCoords coords)
	{
		Debug.Log("Unit built");
		if (_factoryReady.ContainsKey(coords))
			_factoryReady[coords] = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GameEvents.OnMapLoaded -= InitBuildQueue;
		GameEvents.OnUnitBuilt -= OnUnitBuilt;
	}

	private void InitBuildQueue()
	{
		Debug.Log("Init Build Queue");
		_isReady = true;
		_pendingBuildOrders = new Dictionary<int, BuildOrder>();
		_readyToBuildOrders = new List<int>();
		_constructionOrders = new List<ConstructionOrder>();
		_removal = new List<int>();
		GameEvents.OnMapLoaded -= InitBuildQueue;
	}

	protected override void OnUpdate()
	{
		if (!_isReady)
			return;

		Entities.ForEach((Entity e, DynamicBuffer<GenericPrefab> genericPrefabs, DynamicBuffer<TileInstance> tiles) =>
		{
			if (BuildReadyOrders(genericPrefabs, tiles))
				PostUpdateCommands.AddComponent<RecalculateConduitsTag>(GameRegistry.MapEntity);
			ProcessConstructionOrders();
		});
	}

	/// <summary>
	/// Add a building to the build queue
	/// </summary>
	/// <param name="building">The tile info of the building to build</param>
	/// <param name="dst">The tile on which the building will be placed</param>
	public static void QueueBuilding(BuildingTileEntity building, Tile dst, int rotation = 0) => _INST.QueueBuilding(dst, building, rotation);

	/// <summary>
	/// Add a building to the build queue
	/// </summary>
	/// <param name="tile">The tile on which the building will be placed</param>
	/// <param name="building">The tile info of the building to build</param>
	private void QueueBuilding(Tile tile, BuildingTileEntity building, int rotation)
	{
		var orderId = _curOrderID++;
		_pendingBuildOrders.Add(orderId, new BuildOrder
		{
			id = orderId,
			building = building,
			dstTile = tile,
			rotation = rotation,
		});
		_readyToBuildOrders.Add(orderId);
	}

	public void QueueUnit(FactoryBuildingTile factory, UnitIdentifier unit)
	{
		var orderId = _curOrderID++;
		var newOrder = new BuildOrder
		{
			id = orderId,
			factory = factory,
			unit = unit,
			orderType = OrderType.Unit,
		};
		_pendingBuildOrders.Add(orderId, newOrder);
		GameEvents.InvokeOnUnitQueued(newOrder);

		_readyToBuildOrders.Add(orderId);
		if (!_factoryReady.ContainsKey(factory.Coords))
			_factoryReady.Add(factory.Coords, true);
	}

	public void CancelOrder(int orderId)
	{
		if (_pendingBuildOrders.ContainsKey(orderId))
		{
			_pendingBuildOrders.Remove(orderId);
			_readyToBuildOrders.Remove(orderId);
		}
		else
		{
			ConstructionOrder curOrder = default;
			var orderIndex = -1;
			for (int i = 0; i < _constructionOrders.Count; i++)
			{
				if (_constructionOrders[i].id == orderId)
				{
					curOrder = _constructionOrders[i];
					orderIndex = i;
					break;
				}
			}
			if (!curOrder.isCreaated)
			{
				Debug.LogWarning($"No order to cancel with ID: {orderId}");
				return;
			}
			_constructionOrders.RemoveAt(orderIndex);
			((FactoryBuildingTile)GameRegistry.GameMap[curOrder.targetBuilding]).CancelConstruction();
			_factoryReady[curOrder.targetBuilding] = true;
		}
		GameEvents.InvokeOnUnitDequeued(orderId);
	}

	/// <summary>
	/// Place all buildings that have been marked as ready to place
	/// </summary>
	private bool BuildReadyOrders(DynamicBuffer<GenericPrefab> prefabs, DynamicBuffer<TileInstance> tiles)
	{
		int offset = 0;
		var count = 0;
		for (int i = 0; i < _readyToBuildOrders.Count; i++)
		{
			var orderId = _readyToBuildOrders[i - offset];
			var order = _pendingBuildOrders[orderId];

			switch (order.orderType)
			{
				case OrderType.Building:
#if !UNITY_EDITOR //TODO: Remove for final release
					try
					{
#endif
					if (PlaceBuilding(order, prefabs, tiles))
						count++;
#if !UNITY_EDITOR
					}
					catch (Exception e)
					{
						Debug.LogError($"Failed to build building: {order.building.GetNameString()}, skipping");
						Debug.LogError(e);
					}
#endif
					_pendingBuildOrders.Remove(orderId);
					_readyToBuildOrders.RemoveAt(i - offset++);
					break;

				case OrderType.Unit:
					if (_factoryReady.ContainsKey(order.factory.Coords) && _factoryReady[order.factory.Coords]) //Check if can build
					{
						try
						{
							StartUnitConstruction(order);
						}
						catch (Exception e)
						{
							Debug.LogError($"Failed to build unit: {GameRegistry.UnitDatabase[order.unit].info.GetNameString()}, skipping");
							Debug.LogError(e);
						}
						_pendingBuildOrders.Remove(orderId);
						_readyToBuildOrders.RemoveAt(i - offset++);
					}
					break;
			}
		}
		return count > 0;
	}

	private void StartUnitConstruction(BuildOrder order)
	{
		var unit = GameRegistry.UnitDatabase[order.unit];
		_factoryReady[order.factory.Coords] = false;
		order.factory.StartConstruction(unit.info);
		var buildTime = GameRegistry.Cheats.INSTANT_BUILD ? 0 : unit.info.buildTime;
		var constructOrder = new ConstructionOrder(order, buildTime, Time.ElapsedTime + buildTime);
		_constructionOrders.Add(constructOrder);
		GameEvents.InvokeOnUnitConstructionStart(constructOrder);
	}

	/// <summary>
	/// Places a building on the map
	/// </summary>
	/// <param name="order">The build order cotaining the detials on how to place the building</param>
	private bool PlaceBuilding(BuildOrder order, DynamicBuffer<GenericPrefab> prefabs, DynamicBuffer<TileInstance> tileInstances)
	{
		var footprint = order.building.footprint.GetOccupiedTiles(order.dstTile.Coords, order.rotation);
		if (!order.dstTile.IsUnderwater)
			GameRegistry.GameMap.FootprintFlatten(footprint, order.building.flattenOuterRange, Map.FlattenMode.Center | Map.FlattenMode.IgnoreUnderWater);
		var existingTileInstance = tileInstances[order.dstTile.Coords.ToIndex(GameRegistry.GameMap.totalWidth)];
		var building = GameRegistry.GameMap.ReplaceTile(order.dstTile, order.building, order.rotation, prefabs, existingTileInstance, PostUpdateCommands);
		building.CreateMetaTiles(tileInstances, prefabs, PostUpdateCommands);
		var buildTime = GameRegistry.Cheats.INSTANT_BUILD ? 0 : order.building.constructionTime;

		_constructionOrders.Add(new ConstructionOrder(order, buildTime, Time.ElapsedTime + buildTime));
		return order.building is ResourceConduitTileEntity;
	}

	/// <summary>
	/// Checks if the building's construction timer is complete and finishes up the build process
	/// </summary>
	private void ProcessConstructionOrders()
	{
		for (int i = 0; i < _constructionOrders.Count; i++)
		{
			var curOrder = _constructionOrders[i];
			if (Time.ElapsedTime > curOrder.buildCompleteTime)
			{
				switch (curOrder.orderType)
				{
					case OrderType.Building:
						(GameRegistry.GameMap[curOrder.targetBuilding] as BuildingTile).Build();
						break;

					case OrderType.Unit:
						(GameRegistry.GameMap[curOrder.targetBuilding] as FactoryBuildingTile).FinishConstruction();
						GameEvents.InvokeOnUnitConstructionEnd(curOrder.id);
						break;
				}
				_removal.Add(i);
			}
		}
		var offset = 0;
		for (int i = 0; i < _removal.Count; i++)
			_constructionOrders.RemoveAt(_removal[i] - offset++);
		_removal.Clear();
	}
}

public struct BuildOrder
{
	public int id;
	public Tile dstTile;
	public BuildingTileEntity building;
	public FactoryBuildingTile factory;
	public UnitIdentifier unit;
	public OrderType orderType;
	internal int rotation;
}

public enum OrderType
{
	Building,
	Unit
}

public struct ConstructionOrder
{
	public int id;
	public double buildCompleteTime;
	public HexCoords targetBuilding;
	public OrderType orderType;
	public float buildTime;

	public bool isCreaated;

	public ConstructionOrder(BuildOrder order, float buildTime, double completeTime)
	{
		this.id = order.id;
		orderType = order.orderType;
		if (order.orderType == OrderType.Building)
			targetBuilding = order.dstTile.Coords;
		else
			targetBuilding = order.factory.Coords;
		this.buildTime = buildTime;
		buildCompleteTime = completeTime;
		isCreaated = true;
	}
}