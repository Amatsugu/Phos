using Amatsugu.Phos;
using Amatsugu.Phos.TileEntities;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Validators/Turret Placement Validator")]
public class TurretValidator : PlacementValidator
{
	public MeshEntity rangeSphere;
	public MeshEntity minRangeSphere;
	public override bool ValidatePlacement(Map map, HexCoords pos, BuildingTileEntity buildingTile, IndicatorManager indicatorManager, int rotation)
	{
		var turret = buildingTile as TurretTileEntity;
#if DEBUG
		if (turret == null)
			throw new System.Exception($"This tile[{buildingTile.GetType()}] is not a {nameof(TurretTileEntity)}");
#endif
		IndicatorManager.ShowRangeSphere(map[pos], turret.attackRange * 2, rangeSphere);
		if (turret.minAttackRange != 0)
			IndicatorManager.ShowRangeSphere(map[pos], turret.minAttackRange * 2, minRangeSphere);
		return base.ValidatePlacement(map, pos, buildingTile, indicatorManager, rotation);
	}
}
