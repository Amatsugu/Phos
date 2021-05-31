using Amatsugu.Phos.DataStore;
using Amatsugu.Phos.Tiles;

using Unity.Entities;

namespace Amatsugu.Phos
{
	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	public class TileEventsSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
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

			Entities.ForEach((Entity e, DynamicBuffer<BuffEvent> buffEvents) =>
			{
				for (int i = 0; i < buffEvents.Length; i++)
				{
					var curBuff = buffEvents[i];
					var tileInst = tiles[curBuff.tile.ToIndex(width)];
					var tile = map[curBuff.tile] as BuildingTile;
					if (curBuff.remove)
						tile.AddBuff(curBuff.srcTile, curBuff.stats, tileInst, PostUpdateCommands);
					else
						tile.RemoveBuff(curBuff.srcTile, curBuff.stats, tileInst, PostUpdateCommands);
					tile.ApplyBufs(tileInst, PostUpdateCommands);
				}

				buffEvents.Clear();
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
		public StatsBuffs stats;
		public HexCoords srcTile;
		public HexCoords tile;
		public bool remove;
	}
}