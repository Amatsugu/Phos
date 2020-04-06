using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Validators/Resource Gathering Placement Validator")]
public class ResourceGatheringPlacementValidator : PlacementValidator
{
	public MeshEntity gatheringIndicator;
	public MeshEntity cannotGatherIndicator;

	public override bool ValidatePlacement(Map map, HexCoords pos, BuildingTileEntity buildingTile, IndicatorManager indicatorManager)
	{
		var resBuilding = buildingTile as ResourceGatheringBuildingEntity;
		if (resBuilding == null)
			throw new System.Exception();
		var resInRange = new Dictionary<int, int>();
		var resTiles = new Dictionary<int, List<Tile>>();
		map.HexSelectForEach(pos, resBuilding.size + resBuilding.gatherRange, t =>
		{
			if(t is ResourceTile rt && !rt.gatherer.isCreated)
			{
				var yeild = rt.resInfo.resourceYields;
				for (int i = 0; i < yeild.Length; i++)
				{
					var yID = yeild[i].id;
					if (resInRange.ContainsKey(yID))
					{
						resInRange[yID]++;
						resTiles[yID].Add(t);
					}
					else
					{
						resInRange.Add(yID, 1);
						resTiles.Add(yID, new List<Tile> { t });
					}
				}
			}
		}, true);


		return base.ValidatePlacement(map, pos, buildingTile, indicatorManager);
	}
}
