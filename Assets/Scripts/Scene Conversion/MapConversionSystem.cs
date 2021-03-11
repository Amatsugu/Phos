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
				GameRegistry.InitGame(new GameState(map));

				var database = m.tileDatabase;
				GameRegistry.INST.tileDatabase = database;
				var tiles = database.tileEntites.Values.ToArray();
				var buffer = DstEntityManager.AddBuffer<TilePrefab>(mapEntity);
				for (int i = 0; i < tiles.Length; i++)
				{
					var tileDef = tiles[i];
					var e = GetPrimaryEntity(tileDef.tile.tilePrefab);
					buffer.Add(new TilePrefab(tileDef, e));
				}
				DstEntityManager.AddComponent<MapTag>(mapEntity);


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
			Entities.WithNone<Disabled>().WithAll<MapTag>().ForEach((Entity e, DynamicBuffer<TilePrefab> buffer) =>
			{
				var map = GameRegistry.GameMap;
				for (int i = 0; i < map.chunks.Length; i++)
				{
					var chunk = map.chunks[i];
					for (int j = 0; j < chunk.Tiles.Length; j++)
					{
						var tile = chunk.Tiles[j];
						var tileId = GameRegistry.TileDatabase.entityIds[tile.info];
						var prefab = buffer[tileId];
						if(!prefab.isCreated)
						{
							Debug.LogWarning($"Tile {tile.GetNameString()} has not beed created");
							continue;
						}
						if (!EntityManager.Exists(prefab.value))
							continue;
						var inst = PostUpdateCommands.Instantiate(prefab.value);
						var p = tile.SurfacePoint;
						p.y = tile.Height;
						PostUpdateCommands.SetComponent(inst, new Translation { Value = p });
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
	}


	[UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
	public class MapPrefabRegistrationSystem : GameObjectConversionSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((MapAuthoring m) =>
			{
				var mapEntity = GetPrimaryEntity(m);
				var database = m.tileDatabase;
				var tiles = database.tileEntites.Values.ToArray();
				for (int i = 0; i < tiles.Length; i++)
				{
					var tileDef = tiles[i];
					if (tileDef.tile.tilePrefab == null)
						continue;
					DeclareReferencedPrefab(tileDef.tile.tilePrefab);
				}
			});
		}
	}

	public struct MapTag : IComponentData
	{
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
