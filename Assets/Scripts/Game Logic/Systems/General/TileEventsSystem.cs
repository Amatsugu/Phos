using Amatsugu.Phos.DataStore;
using Amatsugu.Phos.Tiles;

using Unity.Entities;

using UnityEngine;

namespace Amatsugu.Phos
{
	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	public class TileEventsSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			var map = GameRegistry.GameMap;
			var width = map.totalWidth;
			Entities.ForEach((Entity e, DynamicBuffer<TileEvent> events, DynamicBuffer<TileInstance> tiles) =>
			{
				for (int i = 0; i < events.Length; i++)
				{
					var curEvent = events[i];
					switch (curEvent.type)
					{
						case TileUpdateType.Placed:
							map[curEvent.tile].OnNeighborPlaced(curEvent, tiles, PostUpdateCommands);
							break;

						case TileUpdateType.Removed:
							map[curEvent.tile].OnNeighborRemoved(curEvent, tiles, PostUpdateCommands);
							break;
					}
				}
				events.Clear();
			});

		}
	}

	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	[UpdateAfter(typeof(TileEventsSystem))]
	public class TileBuffsSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			var map = GameRegistry.GameMap;
			var width = map.totalWidth;
			Entities.ForEach((Entity e, DynamicBuffer<BuffEvent> buffEvents, DynamicBuffer<TileInstance> tiles) =>
			{
				for (int i = 0; i < buffEvents.Length; i++)
				{
					var curBuff = buffEvents[i];
					var tileInst = tiles[curBuff.tile.ToIndex(width)];
					var buidingInst = EntityManager.GetComponentData<Building>(tileInst).Value;
					var tile = map[curBuff.tile] as BuildingTile;
					tile.ApplyBuffs(tileInst, buidingInst, PostUpdateCommands);
				}

				buffEvents.Clear();
			});
			//Entities.WithAllReadOnly<ApplyBuffTag, HexPosition>().ForEach((Entity e, ref HexPosition pos) =>
			//{
			//	(GameRegistry.GameMap[pos.Value] as BuildingTile).ApplyBufs(e, PostUpdateCommands);
			//	PostUpdateCommands.RemoveComponent<ApplyBuffTag>(e);
			//});
		}
	}

	public struct TileEvent : IBufferElementData
	{
		public TileUpdateType type;
		public HexCoords srcTile;
		public HexCoords tile;
	}

	public enum TileUpdateType
	{
		Placed,
		Removed,
		Height
	}

	public struct BuffEvent : IBufferElementData
	{
		public HexCoords tile;
	}

	public struct ApplyBuffTag : IComponentData
	{

	}
}