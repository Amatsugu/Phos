using Amatsugu.Phos.DataStore;
using Amatsugu.Phos.Tiles;

using Unity.Entities;

namespace Amatsugu.Phos
{
	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	public class TileEventsSystem : ComponentSystem
	{
		private bool _isReady = false;

		protected override void OnCreate()
		{
			base.OnCreate();
			GameEvents.OnGameReady += OnReady;
		}

		private void OnReady()
		{
			//_isReady = true;
		}

		protected override void OnUpdate()
		{
			if (!_isReady)
				return;
			var tiles = GameRegistry.GetTileInstanceBuffer();
			var map = GameRegistry.GameMap;
			var width = map.totalWidth;
			Entities.ForEach((Entity e, DynamicBuffer<TileEvent> events) =>
			{
				for (int i = 0; i < events.Length; i++)
				{
					var curEvent = events[i];
					switch (curEvent.type)
					{
						case TileEventType.TilePlaced:
							map[curEvent.tile].OnNeighborPlaced(curEvent, tiles, PostUpdateCommands);
							break;

						case TileEventType.TileRemoved:
							map[curEvent.tile].OnNeighborRemoved(curEvent, tiles, PostUpdateCommands);
							break;
					}
				}
				events.Clear();
			});

			//Entities.ForEach((Entity e, DynamicBuffer<BuffEvent> buffEvents) =>
			//{
			//	for (int i = 0; i < buffEvents.Length; i++)
			//	{
			//		var curBuff = buffEvents[i];
			//		var tileInst = tiles[curBuff.tile.ToIndex(width)];
			//		var tile = map[curBuff.tile] as BuildingTile;
			//		tile.ApplyBufs(tileInst, PostUpdateCommands);
			//	}

			//	buffEvents.Clear();
			//});
		}
	}

	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	[UpdateAfter(typeof(TileEventsSystem))]
	public class TileBuffsSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.WithAllReadOnly<ApplyBuffTag, HexPosition>().ForEach((Entity e, ref HexPosition pos) =>
			{
				(GameRegistry.GameMap[pos.Value] as BuildingTile).ApplyBufs(e, PostUpdateCommands);
				PostUpdateCommands.RemoveComponent<ApplyBuffTag>(e);
			});
		}
	}

	public struct TileEvent : IBufferElementData
	{
		public TileEventType type;
		public HexCoords srcTile;
		public HexCoords tile;
	}

	public enum TileEventType
	{
		TilePlaced,
		TileRemoved
	}

	public struct BuffEvent : IBufferElementData
	{
		public HexCoords tile;
	}

	public struct ApplyBuffTag : IComponentData
	{

	}
}