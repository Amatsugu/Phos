
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
	public class ResourceConduitTile : PoweredBuildingTile
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

		public override void OnHeightChanged()
		{
			base.OnHeightChanged();
			if (!IsBuilt)
				return;
			UpdateLines();
			UpdateConnections(map.conduitGraph.GetConnections(Coords), TileUpdateType.Height);
		}

		[Obsolete]
		public void UpdateLines()
		{
			return;
			var lines = _conduitLines.Keys.ToArray();
			if (!map.conduitGraph.ContainsNode(Coords))
				return;
			var cNode = map.conduitGraph.GetNode(Coords);
			var thisHeight = Coords.WorldPos + new float3(0, cNode.height, 0);
			for (int i = 0; i < lines.Length; i++)
			{
				if (map.conduitGraph.ContainsNode(lines[i]))
				{
					var curNode = map.conduitGraph.GetNode(lines[i]);
					var a = thisHeight;
					var b = curNode.conduitPos.WorldPos + new float3(0, curNode.height, 0);
					if (_switchLines)
					{
						Map.EM.DestroyEntity(_conduitLines[lines[i]]);
						var line = HasHQConnection ? conduitInfo.lineEntity : conduitInfo.lineEntityInactive;
						_conduitLines[lines[i]] = LineFactory.CreateStaticLine(line, a, b);
					}
					else
						LineFactory.UpdateStaticLine(_conduitLines[lines[i]], a, b);
				}
				else
				{
					Map.EM.DestroyEntity(_conduitLines[lines[i]]);
					_conduitLines.Remove(lines[i]);
				}
			}
			_switchLines = false;
		}

		public override void OnPlaced()
		{
			base.OnPlaced();
			map.conduitGraph.AddNodeDisconected(Coords, SurfacePoint.y + conduitInfo.powerLineOffset);
		}

		public override void FindConduitConnections(Entity building, EntityCommandBuffer postUpdateCommands)
		{
			var disconnectedNodesStart = map.conduitGraph.GetDisconectedNodes();
			var closest = map.conduitGraph.GetNodesInRange(Coords, _connectRangeSq);
			var gotConnectedNode = closest.Any(n => map[n.conduitPos] is HQTile || (map[n.conduitPos] as ResourceConduitTile).HasHQConnection);

			var thisNode = map.conduitGraph.GetNode(Coords);

			//Find connections
			var connectionsMade = 0;
			for (int i = 0; i < closest.Count; i++)
			{
				if (thisNode.IsFull)
					break;
				if (closest[i] == thisNode)
					continue;
				if (closest[i].IsFull)
					continue;
				if (closest[i].IsConnectedTo(thisNode))
					continue;
				if ((map[closest[i].conduitPos] as BuildingTile)?.IsBuilt == false)
					continue;

				connectionsMade++;

				thisNode.ConnectTo(closest[i]);
			}
			if (connectionsMade == 0)
				HQDisconnected(building, postUpdateCommands);
			else
			{
				//Propagate Connection
				if (gotConnectedNode)
				{
					var disconnectedNodesEnd = map.conduitGraph.GetDisconectedNodesSet();
					for (int i = 0; i < disconnectedNodesStart.Length; i++)
					{
						if (disconnectedNodesStart[i].conduitPos == Coords)
							continue;
						if (disconnectedNodesEnd.Contains(disconnectedNodesStart[i]))
							continue;
						var tile = map[disconnectedNodesStart[i].conduitPos];
						//if (tile is ResourceConduitTile conduit)
						//	conduit.HQConnected();

						//TODO: Update other conduits
					}
					HQConnected(building, postUpdateCommands);
				}
				else
					HQDisconnected(building, postUpdateCommands);
			}
			connectionInit = true;
		}

		[Obsolete]
		private void AddNewConnections()
		{
			var curNode = map.conduitGraph.GetNode(Coords);
			if (curNode.IsFull)
				return;
			var closest = map.conduitGraph.GetNodesInRange(Coords, _connectRangeSq);
			for (int i = 0; i < closest.Count; i++)
			{
				if (closest[i] == curNode)
					continue;
				if (curNode.IsConnectedTo(closest[i]))
					continue;
				if (!(map[closest[i].conduitPos] as BuildingTile).IsBuilt)
					continue;

				curNode.ConnectTo(closest[i]);
				var tile = map[closest[i].conduitPos];
				//if (tile is ResourceConduitTile conduit && !conduit.HasHQConnection)
				//	conduit.HQConnected();

				//TODO: receive other conduit connections
				var a = closest[i].conduitPos.WorldPos + new float3(0, closest[i].height, 0);
				var b = Coords.WorldPos + new float3(0, curNode.height, 0);
				_conduitLines.Add(closest[i].conduitPos, LineFactory.CreateStaticLine(conduitInfo.lineEntity, a, b));
				if (curNode.IsFull)
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
				PowerTransferEffectSystem.RemoveNode(disconnectedNodes[i]);
			}
			//HQDisconnected();
			UpdateConnections(connections, TileUpdateType.Removed);
			base.OnRemoved();
		}

		protected override void HQConnected(Entity building, EntityCommandBuffer postUpdateCommands)
		{
			if (connectionInit && HasHQConnection)
				return;
			if (!isBuilt)
				return;
			CreateNodeEffect();
			_switchLines = HasHQConnection = true;
			//UpdateLines();
			map.HexSelectForEach(Coords, conduitInfo.poweredRange, t =>
			{
				if (t is ResourceConduitTile)
					return;
				//if (t is PoweredBuildingTile pb)
				//	pb.HQConnected(building, postUpdateCommands);
				//TODO: Update other tiles
			}, true);
			OnConnected();
		}

		private void CreateNodeEffect()
		{
			_energyPacket = conduitInfo.energyPacket.Instantiate(SurfacePoint, Vector3.one * .15f);
			var cNode = map.conduitGraph.GetNode(Coords);
			Map.EM.AddComponentData(_energyPacket, new EnergyPacket
			{
				id = cNode.id,
				progress = -1,
			});
			PowerTransferEffectSystem.AddNode(cNode);
		}

		protected override void HQDisconnected(Entity building, EntityCommandBuffer postUpdateCommands)
		{
			if (connectionInit && !HasHQConnection)
				return;
			if (!isBuilt)
				return;
			HasHQConnection = false;
			_switchLines = true;
			UpdateLines();
			map.HexSelectForEach(Coords, conduitInfo.poweredRange, t =>
			{
				if (t is ResourceConduitTile)
					return;
				//if (t is PoweredBuildingTile pb)
				//	pb.HQDisconnected();
				//Update other tiles
			}, true);
			OnDisconnected();
		}

		public bool IsInPoweredRange(HexCoords tile)
		{
			bool inRange = false;
			map.HexSelectForEach(Coords, conduitInfo.poweredRange, t =>
			{
				if (t.Coords == tile)
				{
					inRange = true;
					return false;
				}
				return true;
			});
			return inRange;
		}

		public bool IsInConnectionRange(HexCoords tile) => Coords.DistanceToSq(tile) <= _connectRangeSq;

		public override void TileUpdated(Tile src, TileUpdateType updateType)
		{
			base.TileUpdated(src, updateType);
			if (!isBuilt)
				return;
			if (_conduitLines.ContainsKey(src.Coords))
				UpdateLines();
			else
				AddNewConnections();
		}

		public void UpdateConnections(ConduitNode[] connections, TileUpdateType updateType)
		{
			if (connections == null)
				return;
			for (int i = 0; i < connections.Length; i++)
			{
				if (connections[i] == null)
					continue;
				map[connections[i].conduitPos].TileUpdated(this, updateType);
			}
		}

		public override StringBuilder GetDescriptionString()
		{
			return base.GetDescriptionString().AppendLine($"Connections Lines: {_conduitLines.Count}")
				.AppendLine($"nConnected Nodes: {map.conduitGraph.GetNode(Coords).ConnectionCount}/{map.conduitGraph.maxConnections}")
				.AppendLine($"nRange {_poweredRangeSq} {HexCoords.TileToWorldDist(conduitInfo.connectionRange, map.innerRadius)}");
		}

		public override void Start()
		{
			base.Start();
			return;
			var thisNode = map.conduitGraph.GetNode(Coords);
			var line = HasHQConnection ? conduitInfo.lineEntity : conduitInfo.lineEntityInactive;
			var b = Coords.WorldPos + new float3(0, thisNode.height, 0);
			var connections = map.conduitGraph.GetConnections(thisNode);
			if (connections == null)
				return;
			for (int i = 0; i < connections.Length; i++)
			{
				var c = connections[i];
				if (_conduitLines.ContainsKey(c.conduitPos))
					continue;
				var a = c.conduitPos.WorldPos + new float3(0, c.height, 0);
				_conduitLines.Add(c.conduitPos, LineFactory.CreateStaticLine(line, a, b));
			}
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