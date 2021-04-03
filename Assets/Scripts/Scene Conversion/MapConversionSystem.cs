using Amatsugu.Phos.Tiles;

using System.Collections.Generic;
using System.Linq;

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
					//Collect Prefabs from decorators
					for (int d = 0; d < tileDef.tile.decorators.Length; d++)
						tileDef.tile.decorators[d].DeclarePrefabs(prefabs);
				}
				DstEntityManager.AddComponent<MapTag>(mapEntity);

				//Convert Building Database
				var buildings = GameRegistry.BuildingDatabase.buildings.Values.OrderBy(v => v.id).ToArray();
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

				DstEntityManager.AddBuffer<TileInstance>(mapEntity);
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
			GameRegistry.GameMap.Destroy();
		}
	}

	/// <summary>
	/// Initialize the buffer to store all the tile instances
	/// </summary>
	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	public class MapLateUpdateSystem : ComponentSystem
	{
		private bool _isDone;

		protected override void OnUpdate()
		{
			if (_isDone)
				return;
			var entities = EntityManager.GetAllEntities();
			var desc = new EntityQueryDesc
			{
				All = new[] { ComponentType.ReadOnly<TileTag>(), ComponentType.ReadOnly<HexPosition>() }
			};
			var query = GetEntityQuery(desc);
			var filter = EntityManager.GetEntityQueryMask(query);

			Entities.WithAllReadOnly<MapTag>().ForEach((Entity e, DynamicBuffer<TileInstance> tiles) =>
			{
				for (int i = 0; i < GameRegistry.GameMap.tileCount; i++)
					tiles.Add(default);
				for (int i = 0; i < entities.Length; i++)
				{
					if (!filter.Matches(entities[i]))
						continue;
					HexCoords coords = EntityManager.GetComponentData<HexPosition>(entities[i]);
					GameRegistry.GameMap[coords].Start(entities[i], PostUpdateCommands);
					tiles[coords.ToIndex(GameRegistry.GameMap.totalWidth)] = entities[i];
				}
				GameRegistry.INST.mapEntity = e;
			});

			_isDone = true;
			entities.Dispose();
			
		}
	}

	public struct MapTag : IComponentData
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