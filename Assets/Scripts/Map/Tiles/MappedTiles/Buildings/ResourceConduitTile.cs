using DataStore.ConduitGraph;
using Effects.Lines;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class ResourceConduitTile : PoweredBuildingTile
{
	public ResourceConduitTileInfo conduitInfo;
	private readonly float _poweredRangeSq;
	private readonly float _connectRangeSq;
	private readonly Dictionary<HexCoords, Entity> _conduitLines;
	private bool _switchLines;

	public ResourceConduitTile(HexCoords coords, float height, ResourceConduitTileInfo tInfo) : base(coords, height, tInfo)
	{
		conduitInfo = tInfo;
		_poweredRangeSq = HexCoords.TileToWorldDist(conduitInfo.poweredRange, Map.ActiveMap.innerRadius);
		_poweredRangeSq *= _poweredRangeSq;
		_connectRangeSq = HexCoords.TileToWorldDist(conduitInfo.connectionRange + 1, Map.ActiveMap.innerRadius);
		_connectRangeSq *= _connectRangeSq;
		_conduitLines = new Dictionary<HexCoords, Entity>();
	}

	public override void OnHeightChanged()
	{
		base.OnHeightChanged();
		UpdateLines();
		UpdateConnections(Map.ActiveMap.conduitGraph.GetConnections(Coords), TileUpdateType.Height);
	}

	public void UpdateLines()
	{
		var lines = _conduitLines.Keys.ToArray();
		for (int i = 0; i < lines.Length; i++)
		{
			var tile = Map.ActiveMap[lines[i]];
			var a = SurfacePoint + conduitInfo.powerLineOffset;
			var b = tile.SurfacePoint + conduitInfo.powerLineOffset;
			if (Map.ActiveMap.conduitGraph.ContainsNode(lines[i]))
			{
				if(_switchLines)
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

	public override void Show(bool isShown)
	{
		base.Show(isShown);
		if (IsShown == isShown)
			return;
		var lines = _conduitLines.Values.ToArray();
		for (int i = 0; i < lines.Length; i++)
		{
			if (isShown)
				Map.EM.AddComponent(lines[i], typeof(FrozenRenderSceneTag));
			else
				Map.EM.RemoveComponent<FrozenRenderSceneTag>(lines[i]);
		}
	}

	public override void FindConduitConnections()
	{
		var disconnectedNodesStart = Map.ActiveMap.conduitGraph.GetDisconectedNodes();
		var closest = Map.ActiveMap.conduitGraph.GetNodesInRange(Coords, _connectRangeSq);
		var nodeCreated = false;
		var gotConnectedNode = closest.Any(n => n != null && (Map.ActiveMap[n.conduitPos] is HQTile || (Map.ActiveMap[n.conduitPos] as ResourceConduitTile).HasHQConnection));


		//Find connections
		var connectionsMade = 0;
		for (int i = 0; i < closest.Count; i++)
		{
			if (connectionsMade >= Map.ActiveMap.conduitGraph.maxConnections)
				break;
			connectionsMade++;
			if (!nodeCreated)
			{
				Map.ActiveMap.conduitGraph.AddNode(Coords, closest[i]);
				nodeCreated = true;
			}
			else
			{
				Map.ActiveMap.conduitGraph.ConnectNode(Coords, closest[i]);
			}
			var tile = Map.ActiveMap[closest[i].conduitPos];
			var a = tile.SurfacePoint + conduitInfo.powerLineOffset;
			var b = SurfacePoint + conduitInfo.powerLineOffset;
			var line = gotConnectedNode ? conduitInfo.lineEntity : conduitInfo.lineEntityInactive;
			_conduitLines.Add(closest[i].conduitPos, LineFactory.CreateStaticLine(line, a, b));
		}
		if (!nodeCreated)
		{
			Map.ActiveMap.conduitGraph.AddNodeDisconected(Coords);
			OnHQDisconnected();
		}
		else
		{
			//Propagate Connection
			if (gotConnectedNode)
			{
				var disconnectedNodesEnd = Map.ActiveMap.conduitGraph.GetDisconectedNodesSet();
				for (int i = 0; i < disconnectedNodesStart.Length; i++)
				{
					if (disconnectedNodesEnd.Contains(disconnectedNodesStart[i]))
						continue;
					var tile = Map.ActiveMap[disconnectedNodesStart[i].conduitPos];
					if (tile is ResourceConduitTile conduit)
						conduit.OnHQConnected();
				}
				OnHQConnected();
			}
			else
				OnHQDisconnected();
		}
		_connectionInit = true;
	}

	void AddNewConnections() //TODO: Work this out
	{
		var curNode = Map.ActiveMap.conduitGraph.GetNode(Coords);
		if (curNode.IsFull)
			return;
		var closest = Map.ActiveMap.conduitGraph.GetNodesInRange(Coords, _connectRangeSq);
		for (int i = 0; i < closest.Count; i++)
		{
			if (closest[i] == curNode)
				continue;
			if (curNode.IsConnectedTo(closest[i]))
				continue;
			curNode.ConnectTo(closest[i]);
			var tile = Map.ActiveMap[closest[i].conduitPos];
			if (tile is ResourceConduitTile conduit && !conduit.HasHQConnection)
				conduit.OnHQConnected();
			var a = tile.SurfacePoint + conduitInfo.powerLineOffset;
			var b = SurfacePoint + conduitInfo.powerLineOffset;
			_conduitLines.Add(closest[i].conduitPos, LineFactory.CreateStaticLine(conduitInfo.lineEntity, a, b));
			if (curNode.IsFull)
				break;
		}
	}

	public override void OnRemoved()
	{
		var connections = Map.ActiveMap.conduitGraph.GetConnections(Coords);
		Map.ActiveMap.conduitGraph.RemoveNode(Coords);
		var disconnectedNodes = Map.ActiveMap.conduitGraph.GetDisconectedNodes();
		for (int i = 0; i < disconnectedNodes.Length; i++)
			(Map.ActiveMap[disconnectedNodes[i].conduitPos] as PoweredBuildingTile).OnHQDisconnected();
		OnHQDisconnected();
		UpdateConnections(connections, TileUpdateType.Removed);
		base.OnRemoved();
	}

	public override void OnHQConnected()
	{
		if (_connectionInit && HasHQConnection)
			return;
		_switchLines = HasHQConnection = true;
		UpdateLines();
		Map.ActiveMap.HexSelectForEach(Coords, conduitInfo.connectionRange, t =>
		{
			if (t is ResourceConduitTile)
				return;
			if (t is PoweredBuildingTile pb)
				pb.OnHQConnected();
		}, true);
	}

	public override void OnHQDisconnected()
	{
		if (_connectionInit && !HasHQConnection)
			return;
		HasHQConnection = false;
		_switchLines = true;
		UpdateLines();
		Map.ActiveMap.HexSelectForEach(Coords, conduitInfo.connectionRange, t =>
		{
			if (t is ResourceConduitTile)
				return;
			if (t is PoweredBuildingTile pb)
				pb.OnHQDisconnected();
		}, true);
	}

	public bool IsInPoweredRange(HexCoords tile)
	{
		bool inRange = false;
		Map.ActiveMap.HexSelectForEach(Coords, conduitInfo.poweredRange, t =>
		{
			if (t.Coords == tile)
			{
				inRange = true;
				return false;
			}
			return true;
		}, true);
		return inRange;
	}
	public bool IsInConnectionRange(HexCoords tile) => Coords.DistanceToSq(tile) <= _connectRangeSq;

	public override void TileUpdated(Tile src, TileUpdateType updateType)
	{
		base.TileUpdated(src, updateType);
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
			Map.ActiveMap[connections[i].conduitPos].TileUpdated(this, updateType);
		}
	}

	public override string GetDescription()
	{
		return base.GetDescription() + $"\nConnections Lines: {_conduitLines.Count}" +
			$"\nConnected Nodes: {Map.ActiveMap.conduitGraph.GetNode(Coords).ConnectionCount}/{Map.ActiveMap.conduitGraph.maxConnections}" +
			$"\nRange {_poweredRangeSq} {HexCoords.TileToWorldDist(conduitInfo.connectionRange, Map.ActiveMap.innerRadius)}";
	}

	public override void Destroy()
	{
		base.Destroy();
		try
		{
			var lines = _conduitLines.Values.ToArray();
			for (int i = 0; i < lines.Length; i++)
			{
				Map.EM.DestroyEntity(lines[i]);
			}
		}
		catch
		{
		}
	}
}