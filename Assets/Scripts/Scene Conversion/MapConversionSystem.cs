using Amatsugu.Phos.Tiles;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
				var database = m.tileDatabase;
				GameRegistry.INST.tileDatabase = database;
				var tiles = database.tileEntites.Values.ToArray();
				var tilesBuffer = DstEntityManager.AddBuffer<TilePrefab>(mapEntity);
				for (int i = 0; i < tiles.Length; i++)
				{
					var tileDef = tiles[i];
					var e = GetPrimaryEntity(tileDef.tile.tilePrefab);
					tilesBuffer.Add(new TilePrefab(tileDef, e));
					tileDef.tile.PrepareEntityPrefab(e, DstEntityManager);
				}
				DstEntityManager.AddComponent<MapTag>(mapEntity);

				//Convert Building Database
				var buildings = GameRegistry.BuildingDatabase.buildings.Values.OrderBy(v => v.id).ToArray();
				var decors = new List<GameObject>();
				var buildingsBuffer = DstEntityManager.AddBuffer<BuildingPrefab>(mapEntity);
				for (int i = 0; i < buildings.Length; i++)
				{
					var building = buildings[i];
					if (building.info.buildingPrefab == null)
						buildingsBuffer.Add(default);
					else
					{
						var e = GetPrimaryEntity(building.info.buildingPrefab);
						buildingsBuffer.Add(new BuildingPrefab(building, e));
					}

					//Collect Prefabs from decorators
					for (int d = 0; d < building.info.decorators.Length; d++)
						building.info.decorators[d].DeclarePrefabs(decors);
				}


				var genericPrefabBuffer = DstEntityManager.AddBuffer<GenericPrefab>(mapEntity);
				var prefabDB = new PrefabDatabase();
				var curPrefabIndex = 0;
				//Collect decors prefabs
				for (int i = 0; i < decors.Count; i++)
				{
					if (prefabDB.RegisterPrefab(decors[i], curPrefabIndex))
					{
						var prefab = GetPrimaryEntity(decors[i]);
						genericPrefabBuffer.Add(new GenericPrefab(prefab));
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
			Map.EM = EntityManager;
		}

		protected override void OnUpdate()
		{
			Entities.WithNone<Disabled>().WithAll<MapTag>().ForEach((Entity e, DynamicBuffer<TilePrefab> tileBuffer, DynamicBuffer<BuildingPrefab> buildingsBuffer) =>
			{
				var map = GameRegistry.GameMap;
				Map.EM = EntityManager;
				var buildingsDic = GameRegistry.BuildingDatabase.buildings.Values.Where(v => v.info.buildingPrefab != null).ToDictionary(v => v.info.buildingPrefab, v => v.id);
				Debug.Log($"Buildings Dict {buildingsDic.Count}");
				for (int i = 0; i < map.chunks.Length; i++)
				{
					var chunk = map.chunks[i];
					for (int j = 0; j < chunk.Tiles.Length; j++)
					{
						var tile = chunk.Tiles[j];
						var tileId = GameRegistry.TileDatabase.entityIds[tile.info];
						if(tile.originalTile != null)
							tileId = GameRegistry.TileDatabase.entityIds[tile.originalTile];
						var prefab = tileBuffer[tileId];
						if(!prefab.isCreated || !EntityManager.Exists(prefab.value))
						{
							Debug.LogWarning($"Tile {tile.GetNameString()} has not beed created");
							continue;
						}
						var tileInst = PostUpdateCommands.Instantiate(prefab.value);
						var p = tile.SurfacePoint;
						p.y = tile.Height;
						PostUpdateCommands.SetComponent(tileInst, new Translation { Value = p });
						PostUpdateCommands.AddComponent(tileInst, new HexPosition { Value = tile.Coords });
						tile.PrepareTileInstance(tileInst, PostUpdateCommands);
						switch(tile)
						{
							case BuildingTile b:
								InstantiateBuilding(tileInst, b, buildingsDic, buildingsBuffer);
								break;
							case ResourceTile r:

								break;
						}

					}
				}

				GameEvents.InvokeOnGameLoaded();
				GameEvents.InvokeOnMapLoaded();
				GameEvents.InvokeOnGameReady();

				EntityManager.AddComponent<Disabled>(e);
			});
		}

		protected override void OnStopRunning()
		{
			base.OnStopRunning();
			GameRegistry.GameMap.Destroy();
		}
		private void InstantiateBuilding(Entity tileInst, BuildingTile buildingTile, Dictionary<GameObject, int> buildingsDic, DynamicBuffer<BuildingPrefab> buildingsBuffer)
		{
			var buildingId = buildingsDic[buildingTile.buildingInfo.buildingPrefab];
			var buildingPrefab = buildingsBuffer[buildingId];
			if (!buildingPrefab.isCreated)
				return;
			var buildingInst = PostUpdateCommands.Instantiate(buildingPrefab.value);
			PostUpdateCommands.AddComponent(buildingInst, new Parent { Value = tileInst });
			PostUpdateCommands.AddComponent<LocalToParent>(buildingInst);
		}
	}


	public struct MapTag : IComponentData
	{
	}

	public struct BuildingPrefab : IBufferElementData
	{
		public int id;
		public Entity value;
		public bool isCreated;

		public BuildingPrefab(BuildingDatabase.BuildingDefination defination, Entity entity)
		{
			id = defination.id;
			value = entity;
			isCreated = true;
		}
	}

	public struct GenericPrefab : IBufferElementData
	{
		public Entity value;

		public GenericPrefab(Entity entity)
		{
			value = entity;
		}
	}

	public struct TilePrefab : IBufferElementData
	{
		public int id;
		public Entity value;
		public bool isCreated;

		public TilePrefab(TileDatabase.TileDefination defination, Entity entity)
		{
			id = defination.id;
			value = entity;
			isCreated = true;
		}

	}
}
