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

	private float _rangeSqr;
	private Dictionary<HexCoords, Entity> _conduitLines;

	public ResourceConduitTile(HexCoords coords, float height, ResourceConduitTileInfo tInfo) : base(coords, height, tInfo)
	{
		conduitInfo = tInfo;
		_rangeSqr = conduitInfo.connectionRange * (2 * Map.ActiveMap.innerRadius) * 2;
		_rangeSqr *= _rangeSqr;
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
				LineFactory.UpdateStaticLine(_conduitLines[lines[i]], a, b);
			else
			{
				Map.EM.DestroyEntity(_conduitLines[lines[i]]);
				_conduitLines.Remove(lines[i]);
			}
		}
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
		var closest = Map.ActiveMap.conduitGraph.GetNodesInRange(Coords, _rangeSqr);
		var nodeCreated = false;
		var gotConnectedNode = false;
		//Find connections
		for (int i = 0; i < closest.Length; i++)
		{
			if (closest[i] != null)
			{
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
				switch(tile)
				{
					case ResourceConduitTile conduit:
						if (conduit.HasHQConnection)
							gotConnectedNode = true;
						break;
					case HQTile _:
						gotConnectedNode = true;
						break;
				}
				var a = tile.SurfacePoint + conduitInfo.powerLineOffset;
				var b = SurfacePoint + conduitInfo.powerLineOffset;
				_conduitLines.Add(closest[i].conduitPos, LineFactory.CreateStaticLine(conduitInfo.lineEntity, a, b));
			}
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
		var closest = Map.ActiveMap.conduitGraph.GetNodesInRange(Coords, _rangeSqr);
		for (int i = 0; i < closest.Length; i++)
		{
			if (closest[i] == null)
				continue;
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

	public override Entity Render()
	{
		return base.Render();
	}

	public override void OnHQConnected()
	{
		if (_connectionInit && HasHQConnection)
			return;
		HasHQConnection = true;
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
		Map.ActiveMap.HexSelectForEach(Coords, conduitInfo.connectionRange, t =>
		{
			if (t is ResourceConduitTile)
				return;
			if (t is PoweredBuildingTile pb)
				pb.OnHQDisconnected();
		}, true);
	}

	public bool IsInRange(HexCoords tile) => Coords.DistanceToSq(tile) <= _rangeSqr;

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
			$"\nConnected Nodes: {Map.ActiveMap.conduitGraph.GetNode(Coords).ConnectionCount}/{Map.ActiveMap.conduitGraph.maxConnections}";
	}

	public override void Destroy()
	{
		var lines = _conduitLines.Values.ToArray();
		for (int i = 0; i < lines.Length; i++)
		{
			Map.EM.DestroyEntity(lines[i]);
		}
		base.Destroy();
	}
}