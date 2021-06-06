
using Amatsugu.Phos.TileEntities;

using DataStore.ConduitGraph;

using Effects.Lines;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
	public class ResourceConduitTile : BuildingTile
	{
		public ResourceConduitTileEntity conduitInfo;
		private readonly float _connectRangeSq;

		public ResourceConduitTile(HexCoords coords, float height, Map map, ResourceConduitTileEntity tInfo, int rotation) : base(coords, height, map, tInfo, rotation)
		{
			conduitInfo = tInfo;
			_connectRangeSq = HexCoords.TileToWorldDist(conduitInfo.connectionRange, map.innerRadius);
			_connectRangeSq *= _connectRangeSq;
		}

		public override void PrepareBuildingEntity(Entity building, EntityCommandBuffer postUpdateCommands)
		{
			base.PrepareBuildingEntity(building, postUpdateCommands);
			postUpdateCommands.AddComponent(building, new ResourceConduitTag { height = conduitInfo.powerLineOffset });
		}


		public override void OnPlaced()
		{
			base.OnPlaced();
			map.conduitGraph.AddNodeDisconected(Coords, conduitInfo.poweredRange, SurfacePoint.y + conduitInfo.powerLineOffset);
		}
		public override void Start(Entity tileInst, EntityCommandBuffer postUpdateCommands)
		{
			Debug.Log("Conduit Start");
			base.Start(tileInst, postUpdateCommands);
			var nodes = map.conduitGraph.GetNodesInRange(Coords, _connectRangeSq);
			int connectCount = 0;
			for (int i = 0; i < nodes.Count; i++)
			{
				if (nodes[i].conduitPos == Coords)
					continue;
				if (nodes[i].IsFull)
					continue;
				map.conduitGraph.ConnectNode(Coords, nodes[i]);
				if (connectCount >= map.conduitGraph.maxConnections)
					break;
			}
		}

		public override void OnRemoved()
		{
			var node = map.conduitGraph.GetNode(Coords);
			Debug.Log($"Remove Node: {node.id}");
			map.conduitGraph.RemoveNode(Coords);
			base.OnRemoved();
		}

		public override void OnDestroy(Entity tileInst, EntityCommandBuffer postUpdateCommands)
		{
			base.OnDestroy(tileInst, postUpdateCommands);
			var desc = new EntityQueryDesc
			{
				None = new[]
				{
					ComponentType.ReadOnly<RecalculateConduitsTag>()
				},
				All = new[]
				{
					ComponentType.ReadOnly<MapTag>()
				}
			};
			var query = GameRegistry.EntityManager.CreateEntityQuery(desc);
			postUpdateCommands.AddComponent<RecalculateConduitsTag>(query);
		}

		public override StringBuilder GetDescriptionString()
		{
			var node = map.conduitGraph.GetNode(Coords);
			return base.GetDescriptionString().AppendLine($"Connections Lines: {node.ConnectionCount}")
				.AppendLine($"nConnected Nodes: {map.conduitGraph.GetNode(Coords).ConnectionCount}/{map.conduitGraph.maxConnections}")
				.AppendLine($"nRange {HexCoords.TileToWorldDist(conduitInfo.connectionRange, map.innerRadius)}")
				.AppendLine($"Connected {map.conduitGraph.GetNode(Coords).IsConnected}");
		}
	}
}