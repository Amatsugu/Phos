using Amatsugu.Phos.Tiles;

using DataStore.ConduitGraph;

using Unity.Entities;

using UnityEngine;

namespace Amatsugu.Phos
{
	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	public class PowerTransferSystem : ComponentSystem
	{
		private ConduitGraph _conduitGraph;

		protected override void OnCreate()
		{
			base.OnCreate();
			GameEvents.OnHQPlaced += OnHQ;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			GameEvents.OnHQPlaced -= OnHQ;
		}

		public void OnHQ()
		{
			_conduitGraph = GameRegistry.GameMap.conduitGraph;
		}

		protected override void OnUpdate()
		{
			if (_conduitGraph == null)
				return;



			Entities.WithAllReadOnly<MapTag, RecalculateConduitsTag>().ForEach(e =>
			{
				_conduitGraph.CalculateConnectivity();
			});

			//TODO: Figure out a better way to defer the deletion of the conduit node to when the entity is destroyed
			Entities.WithAllReadOnly<ResourceConduitTag, HexPosition>().WithNone<HQConntectedTag, PoweredBuildingTag>().ForEach((Entity e, ref HexPosition pos) =>
			{
				if (_conduitGraph.GetNode(pos)?.IsConnected == true)
					PostUpdateCommands.AddComponent<HQConntectedTag>(e);
			});

			Entities.WithAllReadOnly<ResourceConduitTag, HQConntectedTag, HexPosition>().WithNone<PoweredBuildingTag>().ForEach((Entity e, ref HexPosition pos) =>
			{
				if (_conduitGraph.GetNode(pos)?.IsConnected == false)
					PostUpdateCommands.RemoveComponent<HQConntectedTag>(e);
			});

			Entities.WithAllReadOnly<PoweredBuildingTag, HexPosition>().WithNone<BuildingOffTag, ResourceConduitTag>().ForEach((Entity e, ref HexPosition pos) =>
			{
				var node = _conduitGraph.GetClosestPoweredNodeInRange(pos);
				if (node == null)
				{
					(GameRegistry.GameMap[pos.Value] as PoweredBuildingTile).OnConnected();
					PostUpdateCommands.AddComponent<BuildingOffTag>(e);
				}
			});

			Entities.WithAllReadOnly<PoweredBuildingTag, BuildingOffTag, HexPosition>().WithNone<ResourceConduitTag>().ForEach((Entity e, ref HexPosition pos) =>
			{
				var node = _conduitGraph.GetClosestPoweredNodeInRange(pos);
				if (node != null)
				{
					(GameRegistry.GameMap[pos.Value] as PoweredBuildingTile).OnDisconnected();
					PostUpdateCommands.RemoveComponent<BuildingOffTag>(e);
				}
			});
		}
	}

	public struct ResourceConduitTag : IComponentData
	{
		public float height;
	}

	public struct PoweredBuildingTag : IComponentData
	{
	}

	public struct HQConntectedTag : IComponentData
	{
	}

	public struct RecalculateConduitsTag : IComponentData
	{
	}
	public struct RenderConduitLinesTag : IComponentData
	{
	}
}