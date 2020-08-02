using Amatsugu.Phos.TileEntities;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Validators/Turret Placement Validator")]
public class TurretValidator : PlacementValidator
{
	public override bool ValidatePlacement(Map map, HexCoords pos, BuildingTileEntity buildingTile, IndicatorManager indicatorManager)
	{
#if DEBUG
		if (!(buildingTile is TurretTileEntity))
			throw new System.Exception($"This tile[{buildingTile.GetType()}] is not a {nameof(TurretTileEntity)}");
#endif
		return base.ValidatePlacement(map, pos, buildingTile, indicatorManager);
	}
}
