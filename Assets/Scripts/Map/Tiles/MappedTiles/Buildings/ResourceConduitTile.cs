
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
		private readonly float _poweredRangeSq;
		private readonly float _connectRangeSq;
		private readonly Dictionary<HexCoords, Entity> _conduitLines;
		private bool _switchLines;
		private Entity _energyPacket;

		public ResourceConduitTile(HexCoords coords, float height, Map map, ResourceConduitTileEntity tInfo, int rotation) : base(coords, height, map, tInfo, rotation)
		{
			conduitInfo = tInfo;
			_poweredRangeSq = HexCoords.TileToWorldDist(conduitInfo.poweredRange, map.innerRadius);
			_poweredRangeSq *= _poweredRangeSq;
			_connectRangeSq = HexCoords.TileToWorldDist(conduitInfo.connectionRange, map.innerRadius);
			_connectRangeSq *= _connectRangeSq;
			_conduitLines = new Dictionary<HexCoords, Entity>();
		}

		public override void PrepareBuildingEntity(Entity building, EntityCommandBuffer postUpdateCommands)
		{
			base.PrepareBuildingEntity(building, postUpdateCommands);
			postUpdateCommands.AddComponent<ResourceConduitTag>(building);
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
				map.conduitGraph.ConnectNode(Coords, nodes[i]);
				if (connectCount >= map.conduitGraph.maxConnections)
					break;
			}
		}

		public override void OnRemoved()
		{
			var connections = map.conduitGraph.GetConnections(Coords);
			map.conduitGraph.RemoveNode(Coords);
			var disconnectedNodes = map.conduitGraph.GetDisconectedNodes();
			for (int i = 0; i < disconnectedNodes.Length; i++)
			{
				//(map[disconnectedNodes[i].conduitPos] as PoweredBuildingTile).HQDisconnected();
				//PowerTransferEffectSystem.RemoveNode(disconnectedNodes[i]);
			}
			//HQDisconnected();
			base.OnRemoved();
		}



		public override StringBuilder GetDescriptionString()
		{
			return base.GetDescriptionString().AppendLine($"Connections Lines: {_conduitLines.Count}")
				.AppendLine($"nConnected Nodes: {map.conduitGraph.GetNode(Coords).ConnectionCount}/{map.conduitGraph.maxConnections}")
				.AppendLine($"nRange {_poweredRangeSq} {HexCoords.TileToWorldDist(conduitInfo.connectionRange, map.innerRadius)}")
				.AppendLine($"Connected {map.conduitGraph.GetNode(Coords).IsConnected}");
		}

		public override void Dispose()
		{
			base.Dispose();
			if (World.DefaultGameObjectInjectionWorld == null)
				return;
			foreach (var line in _conduitLines)
				Map.EM.DestroyEntity(line.Value);
			Map.EM.DestroyEntity(_energyPacket);
		}
	}
}