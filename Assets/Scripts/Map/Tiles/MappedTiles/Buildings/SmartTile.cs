using Amatsugu.Phos.TileEntities;

using System;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Amatsugu.Phos.Tiles
{
	public class SmartTile : BuildingTile
	{
		public SmartTileEntity smartTile;

		private HashSet<TileEntity> _connectables;
		private NativeArray<Entity> _connectionMeshes;

		public SmartTile(HexCoords coords, float height, Map map, SmartTileEntity tInfo, int rotation) : base(coords, height, map, tInfo, rotation)
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
		}


		/*public override void TileUpdated(Tile src, TileUpdateType updateType)
		{
			base.TileUpdated(src, updateType);
			if (isBuilt)
				*//*RenderConnections();*//*
		}*/


		protected virtual void RenderConnections(DynamicBuffer<GenericPrefab> prefabs, EntityCommandBuffer postupadteCommands)
		{
			var nT = map.GetNeighbors(Coords);
			for (int i = 0; i < 6; i++)
			{
				if (_connectables.Contains(nT[i].info))
				{
					//Add Connection
				}
				else
				{
					//Remove Connection
				}
			}
		}
	}
}