using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;

using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;

[CreateAssetMenu(menuName = "Validators/Conduit Placement Validator")]
public class ConduitPlacementValidator : PlacementValidator
{
	public MeshEntity poweredIndicator;
	public MeshEntityRotatable powerLineIndicator;
	public int maxOverlap = 2;

	public override bool ValidatePlacement(Map map, HexCoords pos, BuildingTileEntity buildingTile, IndicatorManager indicatorManager)
	{
		var conduitInfo = buildingTile as ResourceConduitTileEntity;
		if (conduitInfo == null)
		{
			throw new System.Exception("This tile type cannot use this validator");
		}
		var rangeSqr = HexCoords.TileToWorldDist(conduitInfo.connectionRange, map.innerRadius);
		rangeSqr *= rangeSqr;
		var nodes = map.conduitGraph.GetNodesInRange(pos, rangeSqr);
		while (nodes.Count > map.conduitGraph.maxConnections)
			nodes.RemoveAt(nodes.Count - 1);
		nodes.RemoveAll(n => n.conduitPos == pos);
		var selectedSurface = map[pos].SurfacePoint;
		indicatorManager.ShowLines(powerLineIndicator, selectedSurface + new float3(0, conduitInfo.powerLineOffset, 0), nodes);
#if DEBUG
		for (int i = 0; i < nodes.Count; i++)
		{
			UnityEngine.Debug.DrawLine(selectedSurface, map[nodes[i].conduitPos].SurfacePoint, Color.cyan);
		}
#endif
		//else
		//	HideIndicator(resourceConduitPreviewLine);
		var conduitCount = 0;
		map.HexSelectForEach(pos, conduitInfo.poweredRange, t =>
		{
			indicatorManager.SetIndicator(t, poweredIndicator);
			if (t is ResourceConduitTile)
				conduitCount++;
		}, true);
		if (conduitCount > maxOverlap)
		{
			indicatorManager.SetIndicator(map[pos], errorIndicator);
			indicatorManager.LogError($"Cannot place more than {maxOverlap} overlaping conduits");
			return false;
		}
		return base.ValidatePlacement(map, pos, buildingTile, indicatorManager);
	}
}
