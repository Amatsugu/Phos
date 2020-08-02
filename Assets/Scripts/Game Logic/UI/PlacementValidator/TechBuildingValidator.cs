using Amatsugu.Phos.TileEntities;

using Steamworks;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Validators/Tech Building Validator")]
public class TechBuildingValidator : PlacementValidator
{
	public MeshEntityRotatable borderMesh;
	public override bool ValidatePlacement(Map map, HexCoords pos, BuildingTileEntity buildingTile, IndicatorManager indicatorManager)
	{
		var techBuilding = buildingTile as TechBuildingTileEntity;
#if DEBUG
		if (techBuilding == null)
			throw new System.Exception($"This building is not a {nameof(TechBuildingTileEntity)}");
#endif
		indicatorManager.ShowHexRange(map[pos], techBuilding.effectRange, borderMesh);
		var canBuild = !map.HasTechBuilding(techBuilding);
		if (!canBuild)
			indicatorManager.LogError("Only one tech building of each type can be built");
		return base.ValidatePlacement(map, pos, buildingTile, indicatorManager) && canBuild;
	}
}
