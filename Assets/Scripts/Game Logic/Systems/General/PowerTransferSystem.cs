using DataStore.ConduitGraph;

using Unity.Entities;

using UnityEngine;

namespace Amatsugu.Phos
{
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
			Entities.WithAllReadOnly<ResourceConduitTag>().WithNone<HQConntectedTag, PoweredBuildingTag>().ForEach((Entity e, ref HexPosition pos) =>
			{
				if (_conduitGraph.GetNode(pos).IsConnected)
				{
					PostUpdateCommands.AddComponent<HQConntectedTag>(e);
				}
			});

			Entities.WithAllReadOnly<ResourceConduitTag, HQConntectedTag>().WithNone<PoweredBuildingTag>().ForEach((Entity e, ref HexPosition pos) =>
			{
				if (!_conduitGraph.GetNode(pos).IsConnected)
					PostUpdateCommands.RemoveComponent<HQConntectedTag>(e);
			});

			Entities.WithAllReadOnly<PoweredBuildingTag>().WithNone<BuildingOffTag, ResourceConduitTag>().ForEach((Entity e, ref HexPosition pos) =>
			{
				var closest = _conduitGraph.GetClosestNode(pos);
				if (closest == null)
					return;
				var center = closest.conduitPos;
				for (int k = 0; k <= closest.poweredRange; k++)
				{
					var item = center.Scale(4, k);
					for (int i = 0; i < 6; i++)
					{
						for (int j = 0; j < k; j++)
						{
							if (item == pos)
							{
								PostUpdateCommands.RemoveComponent<BuildingOffTag>(e);
								return;
							}
							item = item.GetNeighbor(i);
						}
					}
				}
			});

			Entities.WithAllReadOnly<PoweredBuildingTag, BuildingOffTag>().WithNone<ResourceConduitTag>().ForEach((Entity e, ref HexPosition pos) =>
			{
				var closest = _conduitGraph.GetClosestConduitNode(pos);
				if (closest == null)
				{
					PostUpdateCommands.AddComponent<BuildingOffTag>(e);
					return;
				}
				var center = closest.conduitPos;
				for (int k = 0; k <= closest.poweredRange; k++)
				{
					var item = center.Scale(4, k);
					for (int i = 0; i < 6; i++)
					{
						for (int j = 0; j < k; j++)
						{
							if (item == pos)
								return;
							item = item.GetNeighbor(i);
						}
					}
				}
				PostUpdateCommands.AddComponent<BuildingOffTag>(e);
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