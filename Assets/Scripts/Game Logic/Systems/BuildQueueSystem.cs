using Amatsugu.Phos;
using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;

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
	}

	private void OnUnitBuilt(HexCoords coords)
	{
		Debug.Log("Unit built");
		if(_factoryReady.ContainsKey(coords))
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
		_INST = this;
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
		BuildReadyOrders();
		ProcessConstructionOrders();
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
			rotation = rotation
		});
		_readyToBuildOrders.Add(orderId);
	}

	public void QueueUnit(FactoryBuildingTile factory, UnitIdentifier unit)
	{
		var orderId = _curOrderID++;
		_pendingBuildOrders.Add(orderId, new BuildOrder
		{
			id = orderId,
			factory = factory,
			unit = unit,
			orderType = OrderType.Unit,
		});

		_readyToBuildOrders.Add(orderId);
		if (!_factoryReady.ContainsKey(factory.Coords))
			_factoryReady.Add(factory.Coords, true);
	}

	/// <summary>
	/// Place all buildings that have been marked as ready to place
	/// </summary>
	private void BuildReadyOrders()
	{
		int offset = 0;
		for (int i = 0; i < _readyToBuildOrders.Count; i++)
		{
			var orderId = _readyToBuildOrders[i - offset];
			var order = _pendingBuildOrders[orderId];
			switch (order.orderType)
			{
				case OrderType.Building:
					PlaceBuilding(order);
					_pendingBuildOrders.Remove(orderId);
					_readyToBuildOrders.RemoveAt(i - offset++);
					break;

				case OrderType.Unit:
					if (_factoryReady.ContainsKey(order.factory.Coords) && _factoryReady[order.factory.Coords]) //Check if can build
					{
						StartUnitConstruction(order);
						_pendingBuildOrders.Remove(orderId);
						_readyToBuildOrders.RemoveAt(i - offset++);
					}
					break;
			}
		}
		//_readyToBuildOrders.Clear();
	}

	private void StartUnitConstruction(BuildOrder order)
	{
		var unit = GameRegistry.UnitDatabase[order.unit];
		_factoryReady[order.factory.Coords] = false;
		order.factory.StartConstruction(unit.info);
		var buildTime = GameRegistry.Cheats.INSTANT_BUILD ? 0 : unit.info.buildTime;
		_constructionOrders.Add(new ConstructionOrder
		{
			orderType = OrderType.Unit,
			id = order.id,
			buildTime = buildTime,
			targetBuilding = order.factory.Coords,
			buildCompleteTime = Time.ElapsedTime + buildTime,
		});
	}

	/// <summary>
	/// Places a building on the map
	/// </summary>
	/// <param name="order">The build order cotaining the detials on how to place the building</param>
	private void PlaceBuilding(BuildOrder order)
	{
		var footprint = order.building.footprint.GetOccupiedTiles(order.dstTile.Coords, order.rotation);
		GameRegistry.GameMap.FootprintFlatten(footprint, order.building.flattenOuterRange, Map.FlattenMode.Center);
		//GameRegistry.GameMap.HexFlatten(order.dstTile.Coords, order.building.footprint.size, order.building.flattenOuterRange, Map.FlattenMode.Average, true);
		GameRegistry.GameMap.ReplaceTile(order.dstTile, order.building, order.rotation);
		var buildTime = GameRegistry.Cheats.INSTANT_BUILD ? 0 : order.building.constructionTime;
		_constructionOrders.Add(new ConstructionOrder
		{
			buildCompleteTime = Time.ElapsedTime + buildTime,
			buildTime = buildTime,
			targetBuilding = order.dstTile.Coords
		});
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
}