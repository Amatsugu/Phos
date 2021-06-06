using Amatsugu.Phos.ECS;
using Amatsugu.Phos.TileEntities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
	public class Tile : IDisposable
	{
		public HexCoords Coords { get; protected set; }
		public float3 SurfacePoint { get; protected set; }
		public float Height { get; protected set; }
		public virtual TileEntity MeshEntity => info;
		public bool IsUnderwater { get; protected set; }

		public readonly TileEntity info;

		public float moisture, temperature;
		public int biomeId;
		public bool IsShown { get; private set; }
		public TileEntity originalTile;
		public readonly Map map;

		protected bool _isRendered;

		/// <summary>
		/// Create a tile
		/// </summary>
		/// <param name="coords">The tile's location on the grid</param>
		/// <param name="height">The height of the tile</param>
		/// <param name="map">The game map that this tile belongs to</param>
		/// <param name="tInfo">The tile Entity that will be used to create the tile</param>
		public Tile(HexCoords coords, float height, Map map, TileEntity tInfo = null)
		{
			Coords = coords;
			Height = height;
			info = tInfo;
			this.map = map;
			if (height < map.seaLevel)
			{
				SurfacePoint = new float3(Coords.WorldPos.x, map.seaLevel, Coords.WorldPos.z);
				IsUnderwater = true;
			}
			else
			{
				IsUnderwater = false;
				SurfacePoint = new float3(Coords.WorldPos.x, Height, Coords.WorldPos.z);
			}
		}

		public Tile(Map map, HexCoords coords, float height, TileDefination tileDefination)
		{
			this.map = map;
			Coords = coords;
			Height = height;
			if (height < map.seaLevel)
			{
				SurfacePoint = new float3(Coords.WorldPos.x, map.seaLevel, Coords.WorldPos.z);
				IsUnderwater = true;
			}
			else
			{
				IsUnderwater = false;
				SurfacePoint = new float3(Coords.WorldPos.x, Height, Coords.WorldPos.z);
			}
		}

		/// <summary>
		/// Callback for when this tile is added to the map
		/// </summary>
		public virtual void OnPlaced()
		{
			BroadcastTileUpdate(TileUpdateType.Placed);
		}

		public TileEntity GetGroundTileInfo()
		{
			return originalTile != null ? originalTile : info;
		}

		public Entity GetTileInstance()
		{
			var tiles = GameRegistry.GetTileInstanceBuffer();
			return tiles[Coords.ToIndex(map.totalWidth)];
		}

		public void InstantiateDecorators(Entity tileInst, ref DynamicBuffer<GenericPrefab> genericPrefabs, EntityCommandBuffer postUpdateCommands)
		{
			for (int i = 0; i < info.decorators.Length; i++)
				info.decorators[i].Instantiate(tileInst, Coords, ref genericPrefabs, postUpdateCommands);
		}

		public virtual Entity InstantiateTile(DynamicBuffer<GenericPrefab> prefabs, EntityCommandBuffer postUpdateCommands)
		{
#if DEBUG
			if (GetGroundTileInfo().tilePrefab == null)
			{
				Debug.LogError($"No prefab for {GetNameString()}: {originalTile.GetNameString()} | {info.GetNameString()}");
				return postUpdateCommands.CreateEntity();
			}
#endif
			var tileId = GameRegistry.PrefabDatabase[GetGroundTileInfo().tilePrefab];
			var prefab = prefabs[tileId];
			var tileInst = postUpdateCommands.Instantiate(prefab.value);
			var p = SurfacePoint;
			p.y = Height;
			postUpdateCommands.SetComponent(tileInst, new Translation { Value = p });
			postUpdateCommands.AddComponent(tileInst, new HexPosition { Value = Coords });
			postUpdateCommands.AddComponent(tileInst, new TileVersion { Value = Time.realtimeSinceStartup });
			postUpdateCommands.AddComponent<TileTag>(tileInst);
			_isRendered = true;
			return tileInst;
		}

		public virtual void PrepareTileInstance(Entity tileInst, EntityCommandBuffer postUpdateCommands)
		{

		}

		public virtual void Start(Entity tileInst, EntityCommandBuffer postUpdateCommands)
		{

		}


		/// <summary>
		/// Set biome data for the tile
		/// </summary>
		/// <param name="biome">Biome ID</param>
		/// <param name="moisture">Moisture level</param>
		/// <param name="temperature">Tempurature</param>
		/// <returns></returns>
		public Tile SetBiome(int biome, float moisture, float temperature)
		{
			this.moisture = moisture;
			this.temperature = temperature;
			biomeId = biome;
			return this;
		}

		/// <summary>
		/// Update the height of a tile
		/// </summary>
		/// <param name="height">The new hieght of the tile</param>
		public void UpdateHeight(float height)
		{
			Height = height;
			if (height < map.seaLevel)
			{
				SurfacePoint = new Vector3(Coords.WorldPos.x, map.seaLevel, Coords.WorldPos.z);
				IsUnderwater = true;
			}
			else
			{
				IsUnderwater = false;
				SurfacePoint = new Vector3(Coords.WorldPos.x, Height, Coords.WorldPos.z);
			}
			var em = GameRegistry.EntityManager;
			var tiles = GameRegistry.GetTileInstanceBuffer();
			var curTile = tiles[Coords.ToIndex(map.totalWidth)];
			em.SetComponentData(curTile, new Translation { Value = new float3(SurfacePoint.x, height, SurfacePoint.z) });
			OnHeightChanged();
			BroadcastTileUpdate(TileUpdateType.Height);
		}

		/// <summary>
		/// Callback triggered when the height of a tile is changed
		/// </summary>
		public virtual void OnHeightChanged()
		{
			if (!_isRendered)
				return;
		}

		/// <summary>
		/// Get the Rich Text name of this tile
		/// </summary>
		/// <returns>String Builder containing the name</returns>
		public virtual StringBuilder GetNameString()
		{
#if UNITY_EDITOR
			return info.GetNameString().Append(" ").Append(Coords);
#else
			return info.GetNameString();
#endif
		}

		/// <summary>
		/// Get the Rich Text description of this tile
		/// </summary>
		/// <returns></returns>
		public virtual StringBuilder GetDescriptionString()
		{
			return new StringBuilder(info.description).AppendLine();
		}

		/// <summary>
		/// Sends a tile update to neighboring tiles
		/// </summary>
		/// <param name="type">The update type</param>
		public virtual void BroadcastTileUpdate(TileUpdateType type)
		{
			var neighbors = map.GetNeighbors(this);
			for (int i = 0; i < 6; i++)
				neighbors[i]?.TileUpdated(this, type);
		}

		/// <summary>
		/// Tile updates enum
		/// </summary>
		public enum TileUpdateType
		{
			Placed,
			Removed,
			Height
		}

		/// <summary>
		/// Receiver for tile updates
		/// </summary>
		/// <param name="src">The tile that send the update</param>
		/// <param name="updateType">The type of update that occured</param>
		public virtual void TileUpdated(Tile src, TileUpdateType updateType)
		{
		}

		public virtual void OnNeighborPlaced(TileEvent tileEvent, DynamicBuffer<TileInstance> tiles, EntityCommandBuffer postUpadteCommands)
		{

		}

		public virtual void OnNeighborRemoved(TileEvent tileEvent, DynamicBuffer<TileInstance> tiles, EntityCommandBuffer postUpadteCommands)
		{

		}

		/// <summary>
		/// Convert this tile to a SerializedTile
		/// </summary>
		/// <returns>A serilaized version of this tile, contains all the information needed to recreate this tile's current state</returns>
		public SerializedTile Serialize()
		{
			var t = new SerializedTile
			{
				tileId = GameRegistry.TileDatabase.entityIds[info],
				origTile = (originalTile == null) ? -1 : GameRegistry.TileDatabase.entityIds[originalTile],
				height = Height,
				x = Coords.X,
				y = Coords.Y,
				tileData = new Dictionary<string, string>()
			};
			OnSerialize(t.tileData);
			return t;
		}

		/// <summary>
		/// Called during to serialization process to write additional data
		/// </summary>
		/// <param name="tileData">Dictionary to write tile data to</param>
		public virtual void OnSerialize(Dictionary<string, string> tileData)
		{

		}

		/// <summary>
		/// Called as the tile is deserialized to read additional data
		/// </summary>
		/// <param name="tileData">Dictionary containg the data to be read</param>
		public virtual void OnDeSerialized(Dictionary<string, string> tileData)
		{

		}
		/// <summary>
		/// Callback for when this tile is being removed from the map
		/// </summary>
		public virtual void OnRemoved()
		{
			BroadcastTileUpdate(TileUpdateType.Removed);
		}

		public virtual void OnDestroy(Entity tileInst, EntityCommandBuffer postUpdateCommands)
		{

		}

		/// <summary>
		/// Destorys all entities associated with this tile and cleans up allocated memory
		/// </summary>
		public virtual void Dispose()
		{
			
		}

		// override object.Equals
		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}

			var t = (Tile)obj;
			return (t.Coords == Coords);
		}

		// override object.GetHashCode
		public override int GetHashCode()
		{
			return Coords.GetHashCode();
		}
	}
}