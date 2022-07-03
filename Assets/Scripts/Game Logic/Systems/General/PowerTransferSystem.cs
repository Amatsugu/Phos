using Amatsugu.Phos.Tiles;

using DataStore.ConduitGraph;

using Unity.Entities;

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

			//Conduits
			//TODO: Figure out a better way to defer the deletion of the conduit node to when the entity is destroyed
			Entities.WithAllReadOnly<ResourceConduitTag, HexPosition>().WithNone<HQConntectedTag, PoweredBuildingTag>().ForEach((Entity e, ref HexPosition pos) =>
			{
				if (_conduitGraph.GetNode(pos) is { IsConnected : true })
					PostUpdateCommands.AddComponent<HQConntectedTag>(e);
			});

			Entities.WithAllReadOnly<ResourceConduitTag, HQConntectedTag, HexPosition>().WithNone<PoweredBuildingTag>().ForEach((Entity e, ref HexPosition pos) =>
			{
				if (_conduitGraph.GetNode(pos) is { IsConnected: false })
					PostUpdateCommands.RemoveComponent<HQConntectedTag>(e);
			});

			//Buildings
			//Powered Buildings
			Entities.WithAllReadOnly<PoweredBuildingTag, HexPosition>().WithNone<BuildingOffTag, ResourceConduitTag, SubTile>().ForEach((Entity e, ref HexPosition pos) =>
			{
				var node = _conduitGraph.GetClosestPoweredNodeInRange(pos);
				if (node == null)
				{
					(GameRegistry.GameMap[pos.Value] as PoweredBuildingTile).OnDisconnected();
					PostUpdateCommands.AddComponent<BuildingOffTag>(e);
				}
			});

			Entities.WithAllReadOnly<PoweredBuildingTag, BuildingOffTag, HexPosition>().WithNone<ResourceConduitTag, SubTile>().ForEach((Entity e, ref HexPosition pos) =>
			{
				var node = _conduitGraph.GetClosestPoweredNodeInRange(pos);
				if (node != null)
				{
					(GameRegistry.GameMap[pos.Value] as PoweredBuildingTile).OnConnected();
					PostUpdateCommands.RemoveComponent<BuildingOffTag>(e);
				}
			});

			//Meta Buildings
			Entities.WithAllReadOnly<PoweredBuildingTag, SubTile, HexPosition>().WithNone<BuildingOffTag, ResourceConduitTag>().ForEach((Entity e, DynamicBuffer<SubTile> tiles, ref HexPosition hPos) =>
			{
				for (int i = 0; i < tiles.Length; i++)
				{
					var pos = tiles[i];
					var node = _conduitGraph.GetClosestPoweredNodeInRange(pos);
					if (node != null) //At lest on powered tile found, abort search
						return;
				}
				//Mark unpowered if no
				(GameRegistry.GameMap[hPos.Value] as PoweredBuildingTile).OnConnected();
				PostUpdateCommands.AddComponent<BuildingOffTag>(e);
			});

			Entities.WithAllReadOnly<PoweredBuildingTag, BuildingOffTag, SubTile, HexPosition>().WithNone<ResourceConduitTag>().ForEach((Entity e, DynamicBuffer<SubTile> tiles, ref HexPosition hPos) =>
			{
				for (int i = 0; i < tiles.Length; i++)
				{
					var pos = tiles[i];
					var node = _conduitGraph.GetClosestPoweredNodeInRange(pos);
					//At least one powered tile found, marking as powered
					if (node != null)
					{
						(GameRegistry.GameMap[hPos.Value] as PoweredBuildingTile).OnDisconnected();
						PostUpdateCommands.RemoveComponent<BuildingOffTag>(e);
						return;
					}
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