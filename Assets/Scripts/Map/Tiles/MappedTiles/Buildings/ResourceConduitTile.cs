using DataStore.ConduitGraph;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ResourceConduitTile : PoweredBuildingTile
{
	public ResourceConduitTileInfo conduitInfo;

	public ResourceConduitTile(HexCoords coords, float height, ResourceConduitTileInfo tInfo) : base(coords, height, tInfo)
	{
		conduitInfo = tInfo;
	}

	public override void OnHeightChanged()
	{
		base.OnHeightChanged();
	}

	public override void OnPlaced()
	{
		base.OnPlaced();
		var rangeSqr = conduitInfo.connectionRange * 2 * Map.ActiveMap.innerRadius;
		rangeSqr *= rangeSqr;
		rangeSqr *= 2;
		var closest = Map.ActiveMap.conduitGraph.GetNodesInRange(Coords, rangeSqr);
		var nodeCreated = false;
		for (int i = 0; i < closest.Length; i++)
		{
			if(closest[i].isCreated)
			{
				if (!nodeCreated)
				{
					Map.ActiveMap.conduitGraph.AddNode(Coords, closest[i]);
					nodeCreated = true;
				}else
					Map.ActiveMap.conduitGraph.ConnectNode(Coords, closest[i]);
				Debug.DrawLine(Map.ActiveMap[closest[i].conduitPos].SurfacePoint, SurfacePoint, Color.magenta, 20);
			}
		}
		if(!nodeCreated)
		{
			Map.ActiveMap.conduitGraph.AddNodeDisconected(nodePos);
		}
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
	}

	public override Entity Render()
	{
		return base.Render();
	}

	public override void OnHQConnected(PoweredBuildingTile src)
	{
		//base.OnHQConnected(src);
	}

	public override void OnHQDisconnected(PoweredBuildingTile src, HashSet<Tile> visited, bool verified = false)
	{
		//base.OnHQDisconnected(src, visited, verified);
	}

	public override void TileUpdated(Tile src, TileUpdateType updateType)
	{
		base.TileUpdated(src, updateType);
	}
}