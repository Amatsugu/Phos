using Amatsugu.Phos.Tiles;

using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

using UnityEngine;

namespace Amatsugu.Phos
{
	public class PrefabBufferInitializationSystem : GameObjectConversionSystem
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
					{
#if DEBUG
						if (tileDef.tile.decorators[d] == null)
						{
							Debug.LogWarning($"Decorator is null for tile {tileDef.tile.GetNameString()}");
							continue;
						}
#endif
						tileDef.tile.decorators[d].DeclarePrefabs(prefabs);
					}
				}
				DstEntityManager.AddComponent<MapTag>(mapEntity);

				//Add buildings to prefab
				var buildings = GameRegistry.BuildingDatabase.buildings.Values.ToArray();
				for (int i = 0; i < buildings.Length; i++)
				{
					var building = buildings[i];
					if (building.info.buildingPrefab != null)
						prefabs.Add(building.info.buildingPrefab);
				}

				//Add units to prefab 
				var units = GameRegistry.UnitDatabase.unitEntites.Values.ToArray();
                for (int i = 0; i < units.Length; i++)
                {
					var unit = units[i];
					if (unit.info.unitPrefab != null)
						prefabs.Add(unit.info.unitPrefab);
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
				GameEvents.InvokeOnGameLoaded();

				//Add Event Buffers
				DstEntityManager.AddBuffer<TileEvent>(mapEntity);
				DstEntityManager.AddBuffer<BuffEvent>(mapEntity);
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


	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	[UpdateAfter(typeof(TileInstanceBufferSystem))]
	public class BuildingInstanceBufferSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.WithAllReadOnly<BuildingInitTag, Parent>().ForEach((Entity e, ref Parent p) =>
			{
				PostUpdateCommands.SetComponent<Building>(p.Value, e);
				PostUpdateCommands.RemoveComponent<BuildingInitTag>(e);
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

		public static implicit operator TileInstance(Entity entity) => new() { Value = entity };

		public static implicit operator Entity(TileInstance inst) => inst.Value;
	}



	public struct MultiTileTag : IComponentData
	{

	}

	public struct SubTile : IBufferElementData
	{
		public HexCoords Value;

		public static implicit operator SubTile(HexCoords coords) => new() { Value = coords };
		public static implicit operator HexCoords(SubTile multiTile) => multiTile.Value;
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