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
	public class Tile
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

		[Obsolete]
		protected Entity _tileEntity;
		[Obsolete]
		private NativeArray<Entity> _decor;
		protected bool _isRendered;
		protected bool _isInit;
		private bool _decorRendered;

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

		/// <summary>
		/// Creates all the entities releated to this tile and shows it in the world
		/// </summary>
		/// <returns></returns>
		[Obsolete]
		public virtual Entity Render()
		{
			IsShown = true;
			_isRendered = true;
			return _tileEntity;
		}

		public virtual void PrepareTileInstance(Entity instance, EntityCommandBuffer postUpdateCommands)
		{

		}

		/// <summary>
		/// Render the decorators for this tile
		/// </summary>
		public virtual void RenderDecorators()
		{
			if (info.decorators.Length == 0)
				return;
			if (_decorRendered)
				return;
			_decorRendered = true;
			_decor = new NativeArray<Entity>(info.decorators.Sum(t => t.GetDecorEntityCount(this)), Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			int lastIndex = 0;
			for (int i = 0; i < info.decorators.Length; i++)
			{
				var count = info.decorators[i].GetDecorEntityCount(this);
				info.decorators[i].Render(this, _decor.Slice(lastIndex, count));
				lastIndex += count;
			}
		}

		/// <summary>
		/// Called after Render and it is safe to perform operations on entities of this tile and other tiles
		/// </summary>
		public virtual void Start()
		{
			_isInit = true;
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
			OnHeightChanged();
			UpdateDecorationHeight();
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
		/// Applies the height of the tile to the decorators
		/// </summary>
		private void UpdateDecorationHeight()
		{
			if (!_isRendered)
				return;
			int lastIndex = 0;
			for (int i = 0; i < info.decorators.Length; i++)
			{
				var count = info.decorators[i].GetDecorEntityCount(this);
				var slice = _decor.Slice(lastIndex, count);
				info.decorators[i].UpdateHeight(slice, this);
				lastIndex += count;
			}
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
		/// <summary>
		/// Sets the visibility of this tile
		/// </summary>
		/// <param name="isShown">Whether or not the tile should be shown</param>
		public void Show(bool isShown)
		{
			IsShown = isShown;
			if (isShown)
				OnShow();
			else
				OnHide();
		}

		/// <summary>
		/// Callback for when the tile marked as visible
		/// </summary>
		public virtual void OnShow()
		{
			Map.EM.RemoveComponent(_decor, typeof(DisableRendering));
		}

		/// <summary>
		/// Callback for when the tile is marked as not visible
		/// </summary>
		public virtual void OnHide()
		{
			Map.EM.AddComponent(_decor, typeof(DisableRendering));
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

		/// <summary>
		/// Destorys all entities associated with this tile and cleans up allocated memory
		/// </summary>
		public virtual void Destroy()
		{
			if (!_isRendered)
				return;
			if(World.DefaultGameObjectInjectionWorld != null)
			{
				Map.EM.DestroyEntity(_tileEntity);
				if(_decor.IsCreated)
					Map.EM.DestroyEntity(_decor);
			}
			if (_decor.IsCreated)
			{
				_decor.Dispose();
			}
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