using Amatsugu.Phos.TileEntities;

using DataStore.ConduitGraph;

using System;

using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
	public class HQTile : BuildingTile
	{
		public readonly HQTileEntity hqInfo;

		public HQTile(HexCoords coords, float height, Map map, HQTileEntity tInfo) : base(coords, height, map, tInfo)
		{
			hqInfo = tInfo;
		}

		public override void OnPlaced()
		{
#if DEBUG
			if (map.HQ != null)
				throw new Exception("Second HQ added");
#endif
			map.HQ = this;
			map.conduitGraph = new ConduitGraph(Coords, Height + 3);
			var tilesToReplace = map.GetNeighbors(Coords);
			for (int i = 0; i < tilesToReplace.Length; i++)
			{
				map.ReplaceTile(tilesToReplace[i], hqInfo.subHQTiles[i]);
			}
			GameRegistry.BaseNameUI.panel.Show();
			GameEvents.InvokeOnHQPlaced();
		}

		protected override void OnBuilt()
		{
			var spawnTiles = map.HexSelect(Coords, 2);
			for (int i = 0; i < spawnTiles.Count; i++)
			{
				if (!(spawnTiles[i] is BuildingTile))
				{
					var b = spawnTiles[i].SurfacePoint;
					b.y = SurfacePoint.y;
					var fwd = SurfacePoint - b;
					var unit = map.AddUnit(hqInfo.unitInfo, spawnTiles[i], hqInfo.faction);
					var rot = new Rotation
					{
						Value = quaternion.LookRotation(fwd, Vector3.up)
					};
					Map.EM.SetComponentData(unit.Entity, rot);
					Map.EM.SetComponentData(unit.HeadEntity, rot);
				}
			}
			//PowerTransferEffectSystem.AddNode(map.conduitGraph.GetNode(Coords));
			ResourceSystem.AddResources(hqInfo.startingResources);
		}

		public override void OnHeightChanged()
		{
			base.OnHeightChanged();
			var foundation = map.HexSelect(Coords, buildingInfo.size);
			for (int i = 0; i < foundation.Count; i++)
			{
				if (foundation[i] != this)
					foundation[i].UpdateHeight(Height);
			}
		}
	}

	public class SubHQTile : PoweredBuildingTile
	{
		public SubHQTile(HexCoords coords, float height, Map map, SubHQTileEntity tInfo) : base(coords, height, map, tInfo)
		{
			HasHQConnection = true;
		}

		public override void OnPlaced()
		{
			base.OnPlaced();
			Build();
		}

		protected override void OnBuilt()
		{
		}

		public override void HQConnected()
		{
		}

		public override void HQDisconnected()
		{
		}
	}
}