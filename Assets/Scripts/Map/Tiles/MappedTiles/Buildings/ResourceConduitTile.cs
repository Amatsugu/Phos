using DataStore.ConduitGraph;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ResourceConduitTile : PoweredBuildingTile
{
	public ResourceConduitTileInfo conduitInfo;

	private float _rangeSqr;

	public ResourceConduitTile(HexCoords coords, float height, ResourceConduitTileInfo tInfo) : base(coords, height, tInfo)
	{
		conduitInfo = tInfo;
		_rangeSqr = conduitInfo.connectionRange * (2 * Map.ActiveMap.innerRadius) * 2;
		_rangeSqr *= _rangeSqr;
	}

	public override void OnHeightChanged()
	{
		base.OnHeightChanged();
	}

	public override void OnPlaced()
	{
		base.OnPlaced();
		
		var closest = Map.ActiveMap.conduitGraph.GetNodesInRange(Coords, _rangeSqr);
		var nodeCreated = false;
		for (int i = 0; i < closest.Length; i++)
		{
			if (closest[i] != null)
			{
				if (!nodeCreated)
				{
					Map.ActiveMap.conduitGraph.AddNode(Coords, closest[i]);
					nodeCreated = true;
				} else
				{
					Map.ActiveMap.conduitGraph.ConnectNode(Coords, closest[i]);
				}
				Debug.DrawLine(Map.ActiveMap[closest[i].conduitPos].SurfacePoint, SurfacePoint, Color.magenta, 20);
			}
		}
		if (!nodeCreated)
		{
			Map.ActiveMap.conduitGraph.AddNodeDisconected(Coords);
			OnHQDisconnected();
		}
		else
			OnHQConnected();
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		Map.ActiveMap.conduitGraph.RemoveNode(Coords);
		var disconnectedNodes = Map.ActiveMap.conduitGraph.GetDisconectedNodes();
		for (int i = 0; i < disconnectedNodes.Length; i++)
			(Map.ActiveMap[disconnectedNodes[i].conduitPos] as PoweredBuildingTile).OnHQDisconnected();
		OnHQDisconnected();
	}

	public override Entity Render()
	{
		return base.Render();
	}

	public override void OnHQConnected()
	{
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
		HasHQConnection = true;
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
	}
}