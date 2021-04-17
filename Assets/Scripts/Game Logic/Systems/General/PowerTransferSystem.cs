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
			_conduitGraph.CalculateConnectivity();
			Entities.WithAllReadOnly<ResourceConduitTag, HexPosition>().WithNone<HQConntectedTag, PoweredBuildingTag>().ForEach((Entity e, ref HexPosition pos) =>
			{
				if (_conduitGraph.GetNode(pos).IsConnected)
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
					PostUpdateCommands.AddComponent<BuildingOffTag>(e);
			});

			Entities.WithAllReadOnly<PoweredBuildingTag, BuildingOffTag, HexPosition>().WithNone<ResourceConduitTag>().ForEach((Entity e, ref HexPosition pos) =>
			{
				var node = _conduitGraph.GetClosestPoweredNodeInRange(pos);
				if (node != null)
					PostUpdateCommands.RemoveComponent<BuildingOffTag>(e);
			});
		}
	}

	public struct ResourceConduitTag : IComponentData
	{
	}

	public struct PoweredBuildingTag : IComponentData
	{
	}

	public struct HQConntectedTag : IComponentData
	{
	}
}