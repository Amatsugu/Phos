using Amatsugu.Phos.TileEntities;

using DataStore.ConduitGraph;

using System;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
	public class HQTile : BuildingTile
	{
		public readonly HQTileEntity hqInfo;

		public HQTile(HexCoords coords, float height, Map map, HQTileEntity tInfo) : base(coords, height, map, tInfo, 0)
		{
			hqInfo = tInfo;
		}

		public override void OnPlaced()
		{
#if DEBUG
			if (map.conduitGraph != null)
				throw new Exception("Second HQ added");
#endif
			map.conduitGraph = new ConduitGraph(Coords, Height + 3);
			var tilesToReplace = map.GetNeighbors(Coords);
			for (int i = 0; i < tilesToReplace.Length; i++)
			{
				//map.ReplaceTile(tilesToReplace[i], hqInfo.subHQTiles[i]);
			}
			GameRegistry.BaseNameUI.panel.Show();
			GameEvents.InvokeOnHQPlaced();
		}

		public override Entity InstantiateTile(DynamicBuffer<GenericPrefab> prefabs, EntityCommandBuffer postUpdateCommands)
		{
			var tilesToReplace = map.GetNeighbors(Coords);
			for (int i = 0; i < tilesToReplace.Length; i++)
			{
				BuildQueueSystem.QueueBuilding(hqInfo.subHQTiles[i], tilesToReplace[i]);
			}
			return base.InstantiateTile(prefabs, postUpdateCommands);
		}

		protected override void OnBuilt()
		{
			//TODO: Create starter units
			ResourceSystem.AddResources(hqInfo.startingResources);
		}

		public override void OnHeightChanged()
		{
			base.OnHeightChanged();
			var foundation = map.HexSelect(Coords, buildingInfo.footprint.size);
			for (int i = 0; i < foundation.Count; i++)
			{
				if (foundation[i] != this)
					foundation[i].UpdateHeight(Height);
			}
		}

		public override bool CanDeconstruct(Faction faction) => false;

	}

	public class SubHQTile : PoweredBuildingTile
	{
		public SubHQTile(HexCoords coords, float height, Map map, SubHQTileEntity tInfo) : base(coords, height, map, tInfo, 0)
		{
			HasHQConnection = true;
		}

		public override void OnPlaced()
		{
			base.OnPlaced();
		}

		protected override void OnBuilt()
		{
		}


		public override bool CanDeconstruct(Faction faction) => false;
	}
}