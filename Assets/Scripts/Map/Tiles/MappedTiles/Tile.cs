using Amatsugu.Phos.TileEntities;

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

		protected Entity _tileEntity;
		private NativeArray<Entity> _decor;
		protected bool _isRendered;
		protected bool _isInit;
		private bool _decorRendered;

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

		public Tile SetBiome(int biome, float moisture, float temperature)
		{
			this.moisture = moisture;
			this.temperature = temperature;
			biomeId = biome;
			return this;
		}

		public virtual StringBuilder GetNameString()
		{
#if UNITY_EDITOR
			return info.GetNameString().Append(" ").Append(Coords);
#else
			return info.GetNameString();
#endif
		}

		public virtual StringBuilder GetDescriptionString()
		{
			return new StringBuilder(info.description).AppendLine();
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
			UpdateDecorations();
			BroadcastTileUpdate(TileUpdateType.Height);
		}

		public virtual void OnHeightChanged()
		{
			if (!_isRendered)
				return;

			if (info.mesh != null)
				Map.EM.SetComponentData(_tileEntity, new Translation { Value = SurfacePoint });
		}

		private void UpdateDecorations()
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

		public virtual void OnRemoved()
		{
		
			BroadcastTileUpdate(TileUpdateType.Removed);
		}

		public virtual void OnPlaced()
		{
			BroadcastTileUpdate(TileUpdateType.Placed);
		}

		public virtual void BroadcastTileUpdate(TileUpdateType type)
		{
			var neighbors = map.GetNeighbors(this);
			for (int i = 0; i < 6; i++)
				neighbors[i]?.TileUpdated(this, type);
		}

		public enum TileUpdateType
		{
			Placed,
			Removed,
			Height
		}

		public virtual void TileUpdated(Tile src, TileUpdateType updateType)
		{
		}

		public virtual void Destroy()
		{
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

		public void Show(bool isShown)
		{
			IsShown = isShown;
			if (isShown)
				OnShow();
			else
				OnHide();
		}

		public virtual void OnShow()
		{
			Map.EM.RemoveComponent(_decor, typeof(DisableRendering));
		}

		public virtual void OnHide()
		{
			Map.EM.AddComponent(_decor, typeof(DisableRendering));
		}


		public virtual Entity Render()
		{
			IsShown = true;
			_isRendered = true;
			_tileEntity = MeshEntity.Instantiate(Coords, Height);
			return _tileEntity;
		}

		public virtual void Start()
		{
			_isInit = true;
		}

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

		public virtual void OnSerialize(Dictionary<string, string> tileData)
		{

		}

		public virtual void OnDeSerialized(Dictionary<string, string> tileData)
		{

		}

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
	}
}