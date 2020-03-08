﻿using AnimationSystem.AnimationData;
using AnimationSystem.Animations;

using System.Collections.Generic;

using Unity.Entities;
using Unity.Mathematics;

using Random = UnityEngine.Random;

public class BuildQueueSystem : ComponentSystem
{
	private Dictionary<int, BuildOrder> _pendingBuildOrders;
	private List<int> _readyToBuildOrders;
	private List<ConstructionOrder> _constructionOrders;
	private List<int> _removal;

	private static BuildQueueSystem _INST;

	protected override void OnCreate()
	{
		_INST = this;
		_pendingBuildOrders = new Dictionary<int, BuildOrder>();
		_readyToBuildOrders = new List<int>();
		_constructionOrders = new List<ConstructionOrder>();
		_removal = new List<int>();
	}

	protected override void OnUpdate()
	{
		BuildReadyBuildings();
		ProcessConstructionOrders();
	}

	private void ProcessConstructionOrders()
	{
		for (int i = 0; i < _constructionOrders.Count; i++)
		{
			var curOrder = _constructionOrders[i];
			if (Time.ElapsedTime > curOrder.buildTime)
			{
				(Map.ActiveMap[curOrder.building] as BuildingTile).Build();
				_removal.Add(i);
			}
		}
		var offset = 0;
		for (int i = 0; i < _removal.Count; i++)
			_constructionOrders.RemoveAt(_removal[i] - offset++);
		_removal.Clear();
	}

	public static void QueueBuilding(BuildingTileInfo building, Tile dst, MeshEntityRotatable dropPod)
	{
		_INST.QueueBuilding(dst, building, dropPod);
	}

	private void QueueBuilding(Tile tile, BuildingTileInfo building, MeshEntityRotatable dropPod)
	{
		var hqMode = building is HQTileInfo;
		var callback = tile.Coords.GetHashCode();
		_pendingBuildOrders.Add(callback, new BuildOrder
		{
			building = building,
			dstTile = tile
		});
		if (hqMode)
		{
			var pos = tile.SurfacePoint;
			pos.y = Random.Range(90, 100);
			var e = dropPod.Instantiate(pos);
			EntityManager.AddComponentData(e, new FallAnim
			{
				startSpeed = new float3(0, Random.Range(-100, -90), 0)
			});
			EntityManager.AddComponentData(e, new Floor
			{
				Value = tile.Height
			});
			EntityManager.AddComponentData(e, new HitFloorCallback
			{
				eventId = callback
			});
			EntityManager.AddComponentData(e, new Gravity { Value = 9.8f });
			EventManager.AddEventListener(callback.ToString(), () =>
			{
				_readyToBuildOrders.Add(callback);
			});
		}
		else
		{
			_readyToBuildOrders.Add(callback);
		}
	}

	private void BuildReadyBuildings()
	{
		for (int i = 0; i < _readyToBuildOrders.Count; i++)
		{
			var orderId = _readyToBuildOrders[i];
			EventManager.RemoveAllEventListeners(orderId.ToString());
			PlaceBuilding(_pendingBuildOrders[orderId]);
			_pendingBuildOrders.Remove(orderId);
		}
		_readyToBuildOrders.Clear();
	}

	private void PlaceBuilding(BuildOrder order)
	{
		Map.ActiveMap.HexFlatten(order.dstTile.Coords, order.building.size, order.building.flattenOuterRange, Map.FlattenMode.Average, true);
		Map.ActiveMap.ReplaceTile(order.dstTile, order.building);
		_constructionOrders.Add(new ConstructionOrder
		{
			buildTime = GameRegistry.Cheats.INSTANT_BUILD ? Time.ElapsedTime : Time.ElapsedTime + order.building.constructionTime,
			building = order.dstTile.Coords
		});
	}
}

public struct BuildOrder
{
	public Tile dstTile;
	public BuildingTileInfo building;
}

public struct ConstructionOrder
{
	public double buildTime;
	public HexCoords building;
}