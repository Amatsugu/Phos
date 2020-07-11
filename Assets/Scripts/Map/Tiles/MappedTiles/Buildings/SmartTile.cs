using Amatsugu.Phos.TileEntities;

using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
	public class SmartTile : BuildingTile
	{
		public SmartTileEntity smartTile;

		private HashSet<TileEntity> _connectables;
		private NativeArray<Entity> _connectionMeshes;

		public SmartTile(HexCoords coords, float height, Map map, SmartTileEntity tInfo = null) : base(coords, height, map, tInfo)
		{
			smartTile = tInfo;
			_connectables = new HashSet<TileEntity>();
			_connectionMeshes = new NativeArray<Entity>(6, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			if (tInfo.connectToSelf)
				_connectables.Add(tInfo);
			for (int i = 0; i < tInfo.connectTo.Length; i++)
				_connectables.Add(GameRegistry.BuildingDatabase.buildings[tInfo.connectTo[i].id].info);
		}
		public override void OnHeightChanged()
		{
			base.OnHeightChanged();
			RenderConnections();
		}

		public override void OnHide()
		{
			base.OnHide();
			for (int i = 0; i < 6; i++)
			{
				if(Map.EM.Exists(_connectionMeshes[i]))
					Map.EM.AddComponent<FrozenRenderSceneTag>(_connectionMeshes[i]);
			}
		}

		public override void OnShow()
		{
			base.OnShow();
			for (int i = 0; i < 6; i++)
			{
				if (Map.EM.Exists(_connectionMeshes[i]))
					Map.EM.RemoveComponent<FrozenRenderSceneTag>(_connectionMeshes[i]);
			}
		}

		public override void TileUpdated(Tile src, TileUpdateType updateType)
		{
			base.TileUpdated(src, updateType);
			RenderConnections();
		}

		public override void RenderBuilding()
		{
			RenderConnections();
			base.RenderBuilding();
		}

		protected virtual void RenderConnections()
		{
			var nT = map.GetNeighbors(Coords);
			for (int i = 0; i < 6; i++)
			{
				if(_connectables.Contains(nT[i].info))
				{
					if (!Map.EM.Exists(_connectionMeshes[i]))
						_connectionMeshes[i] = smartTile.connectionMesh.Instantiate(SurfacePoint, 1, quaternion.RotateY(math.radians(90 + (60 * i))));
					else
						Map.EM.SetComponentData(_connectionMeshes[i], new Translation { Value = SurfacePoint });
				}else
				{
					if (Map.EM.Exists(_connectionMeshes[i]))
						Map.EM.DestroyEntity(_connectionMeshes[i]);
				}
			}
		}
		public override void Destroy()
		{
			base.Destroy();
			Map.EM.DestroyEntity(_connectionMeshes);
		}
	}
}