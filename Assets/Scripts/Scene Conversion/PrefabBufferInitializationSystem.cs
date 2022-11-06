using Amatsugu.Phos.TileEntities;

using System.Collections.Generic;
using System.Linq;

using Unity.Entities;
using Unity.Transforms;

using UnityEngine;

namespace Amatsugu.Phos
{
	/// <summary>
	/// Instantiate object prefabs and add to databases
	/// </summary>
	public class PrefabBufferInitializationSystem : GameObjectConversionSystem
	{
		protected override void OnUpdate()
		{
			Debug.Log("Initializing Entity Prefabs...");
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
					if (tileDef.tile is BuildingTileEntity b && b.validator != null)
						prefabs.AddRange(b.validator.GetIndicatorPrefabs());
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
					{
						var e = GetPrimaryEntity(unit.info.unitPrefab);
						unit.info.PrepareComponentData(e, PostUpdateCommands);

						prefabs.AddRange(unit.info.GetPrefabs());
					}
                }

				prefabs.AddRange(GameRegistry.PrefabsToInit);

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
			Debug.Log("Entity Prefabs Initialized");

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

		public static implicit operator Entity(GenericPrefab prefab) => prefab.value;
		public static implicit operator GenericPrefab(Entity entity) => new(entity);

	}
}