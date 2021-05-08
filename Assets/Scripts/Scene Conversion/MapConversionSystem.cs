using Amatsugu.Phos.Tiles;

using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace Amatsugu.Phos
{
	public class MapConversionSystem : GameObjectConversionSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((MapAuthoring m) =>
			{
				var mapEntity = GetPrimaryEntity(m);
				var map = m.generator.GenerateMap();
				m.generator.GenerateFeatures(map);
				GameRegistry.InitGame(new GameState(map));

				var tilesBuffer = DstEntityManager.AddBuffer<TileInstance>(mapEntity);
				for (int i = 0; i < map.tileCount; i++)
					tilesBuffer.Add(default);

				//Convert Tile Database
				var prefabDB = GameRegistry.INST.prefabDatabase = new PrefabDatabase();
				var curPrefabIndex = 0;
				var prefabs = new List<GameObject>();

				var tileDB = GameRegistry.INST.tileDatabase = m.tileDatabase;
				var tiles = tileDB.tileEntites.Values.OrderBy(t => t.id).ToArray();

				for (int i = 0; i < tiles.Length; i++)
				{
					var tileDef = tiles[i];
					prefabs.Add(tileDef.tile.tilePrefab);
					var e = GetPrimaryEntity(tileDef.tile.tilePrefab);
					tileDef.tile.PrepareEntityPrefab(e, DstEntityManager);
					if (DstEntityManager.Exists(e))
						DstEntityManager.AddComponent<NewInstanceTag>(e);
					//Collect Prefabs from decorators
					for (int d = 0; d < tileDef.tile.decorators.Length; d++)
						tileDef.tile.decorators[d].DeclarePrefabs(prefabs);
				}
				DstEntityManager.AddComponent<MapTag>(mapEntity);

				//Convert Building Database
				var buildings = GameRegistry.BuildingDatabase.buildings.Values.ToArray();
				for (int i = 0; i < buildings.Length; i++)
				{
					var building = buildings[i];
					if (building.info.buildingPrefab != null)
						prefabs.Add(building.info.buildingPrefab);
				}

				var genericPrefabBuffer = DstEntityManager.AddBuffer<GenericPrefab>(mapEntity);
				Debug.Log($"Prefabs to register {prefabs.Count}");
				//Collect prefabs and register to db
				for (int i = 0; i < prefabs.Count; i++)
				{
					if (prefabs[i] == null)
						continue;
					if (prefabDB.RegisterPrefab(prefabs[i], curPrefabIndex))
					{
						Debug.Log($"Registering {prefabs[i].name}");
						var prefab = GetPrimaryEntity(prefabs[i]);
						genericPrefabBuffer.Add(new GenericPrefab(prefab));
						curPrefabIndex++;
					}
				}
			});
		}
	}

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

				GameEvents.InvokeOnGameLoaded();
				GameEvents.InvokeOnMapLoaded();
				for (int i = 0; i < map.chunks.Length; i++)
				{
					var chunk = map.chunks[i];
					for (int j = 0; j < chunk.Tiles.Length; j++)
					{
						var tile = chunk.Tiles[j];
						//tile.Start();
					}
				}

				GameEvents.InvokeOnGameReady();

				EntityManager.AddComponent<MapGeneratedTag>(e);
			});
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			//GameRegistry.GameMap.Destroy();
		}
	}

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
				GameRegistry.INST.mapEntity = e;
				PostUpdateCommands.AddComponent<MapInitTag>(e);
			});
			_entities.Dispose();
		}
	}

	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	public class TileInstanceBufferSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			var buffer = EntityManager.GetBuffer<TileInstance>(GameRegistry.MapEntity);
			var mapWidth = GameRegistry.GameMap.totalWidth;
			Entities.WithAllReadOnly<NewInstanceTag, HexPosition, TileTag>().ForEach((Entity e, ref HexPosition pos) =>
			{
				buffer[pos.Value.ToIndex(mapWidth)] = e;
				PostUpdateCommands.RemoveComponent<NewInstanceTag>(e);
			});
		}
	}

	public struct NewInstanceTag : IComponentData
	{
	}

	public struct MapTag : IComponentData
	{
	}

	public struct MapInitTag : IComponentData
	{
	}

	public struct MapGeneratedTag : IComponentData
	{
	}

	public struct TileTag : IComponentData
	{
	}

	public struct TileInstance : IBufferElementData
	{
		public Entity Value;

		public static implicit operator TileInstance(Entity entity) => new TileInstance { Value = entity };

		public static implicit operator Entity(TileInstance inst) => inst.Value;
	}

	public struct TileVersion : IComponentData
	{
		public float Value;
	}

	public struct GenericPrefab : IBufferElementData
	{
		public Entity value;

		public GenericPrefab(Entity entity)
		{
			value = entity;
		}
	}
}