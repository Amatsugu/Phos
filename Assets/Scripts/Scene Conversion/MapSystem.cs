using Amatsugu.Phos.Tiles;

using Unity.Entities;

namespace Amatsugu.Phos
{
	/// <summary>
	/// Instantiates all tile entities and prepares the map for use in the scene
	/// </summary>
	public class MapSystem : ComponentSystem
	{
		protected override void OnStartRunning()
		{
			base.OnStartRunning();
			GameRegistry.INST.entityManager = EntityManager;
		}

		protected override void OnUpdate()
		{
			Entities.WithNone<MapGeneratedTag>().WithAll<MapTag>().ForEach((Entity e, DynamicBuffer<GenericPrefab> genericPrefabs, DynamicBuffer<TileInstance> tiles) =>
			{
				var map = GameRegistry.GameMap;
				GameRegistry.INST.mapEntity = e;
				//tiles.EnsureCapacity(map.tileCount);
				for (int z = 0; z < map.totalHeight; z++)
				{
					for (int x = 0; x < map.totalWidth; x++)
					{
						var tile = map[HexCoords.FromOffsetCoords(x, z, map.tileEdgeLength)];
						var tileInst = tile.InstantiateTile(genericPrefabs, PostUpdateCommands);
						tile.PrepareTileInstance(tileInst, PostUpdateCommands);
						tile.InstantiateDecorators(tileInst, ref genericPrefabs, PostUpdateCommands);
						switch (tile)
						{
							case BuildingTile b:
								var buildingInst = b.InstantiateBuilding(tileInst, genericPrefabs, PostUpdateCommands);
								b.PrepareBuildingEntity(buildingInst, PostUpdateCommands);
								break;
						}
						tiles.Add(tileInst);
					}
				}

				for (int i = 0; i < map.chunks.Length; i++)
				{
					var chunk = map.chunks[i];
					for (int j = 0; j < chunk.Tiles.Length; j++)
					{
						var tile = chunk.Tiles[j];
						//tile.Start();
					}
				}

				EntityManager.AddComponent<MapGeneratedTag>(e);

				GameEvents.InvokeOnMapLoaded();
			});
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			//GameRegistry.GameMap.Destroy();
		}
	}
}