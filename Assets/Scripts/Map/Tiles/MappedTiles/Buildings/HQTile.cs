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
				throw new Exception("A conduit graph already exists, did the previous one not get cleaned up? Are there multiple HQs?");
#endif
			map.conduitGraph = new ConduitGraph(Coords, Height + 3);
			GameRegistry.BaseNameUI.panel.Show();
			GameEvents.InvokeOnHQPlaced();
		}

		public override Entity InstantiateTile(DynamicBuffer<GenericPrefab> prefabs, EntityCommandBuffer postUpdateCommands)
		{
			var tilesToReplace = map.GetNeighbors(Coords);
			for (int i = 0; i < tilesToReplace.Length; i++)
				BuildQueueSystem.QueueBuilding(hqInfo.subHQTiles[i], tilesToReplace[i]);
			var spawn = HexCoords.SelectRing(Coords, 2);
			var fac = GameRegistry.EntityManager.World.GetOrCreateSystem<UnitFactorySystem>();
            for (int i = 0; i < spawn.Length; i++)
				fac.BuildUnit(hqInfo.unitInfo, map[spawn[i]].SurfacePoint, Faction.Player, 0.100 * i);
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

        protected override void SendBuildNotification()
        {
        }

    }
}