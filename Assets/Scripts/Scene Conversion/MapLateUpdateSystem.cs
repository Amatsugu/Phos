using Amatsugu.Phos.Tiles;

using Unity.Collections;
using Unity.Entities;

namespace Amatsugu.Phos
{
	/// <summary>
	/// Initialize the buffer to store all the tile instances
	/// </summary>
	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	[UpdateBefore(typeof(TileInstanceBufferSystem))]
	public class MapLateUpdateSystem : ComponentSystem
	{
		private NativeArray<Entity> _entities;
		private EntityQuery _query;

		protected override void OnStartRunning()
		{
			_entities = EntityManager.GetAllEntities();
			var desc = new EntityQueryDesc
			{
				All = new[] { ComponentType.ReadOnly<TileTag>(), ComponentType.ReadOnly<HexPosition>(), ComponentType.ReadOnly<NewInstanceTag>() }
			};
			_query = GetEntityQuery(desc);
		}

		protected override void OnUpdate()
		{
			Entities.WithAllReadOnly<MapTag>().WithNone<MapInitTag>().ForEach((Entity e, DynamicBuffer<TileInstance> tiles) =>
			{
				var filter = EntityManager.GetEntityQueryMask(_query);
				for (int i = 0; i < GameRegistry.GameMap.tileCount; i++)
					tiles.Add(default);
				for (int i = 0; i < _entities.Length; i++)
				{
					if (!filter.Matches(_entities[i]))
						continue;
					HexCoords coords = EntityManager.GetComponentData<HexPosition>(_entities[i]);
					var tile = GameRegistry.GameMap[coords];
					PostUpdateCommands.RemoveComponent<NewInstanceTag>(_entities[i]);
					tile.Start(_entities[i], PostUpdateCommands);
					if (tile is BuildingTile buildingTile)
						buildingTile.BuildingStart(_entities[i], PostUpdateCommands);
					tiles[coords.ToIndex(GameRegistry.GameMap.totalWidth)] = _entities[i];
				}
				GameEvents.InvokeOnGameReady();
				PostUpdateCommands.AddComponent<MapInitTag>(e);
				_entities.Dispose();
			});
		}
	}
}