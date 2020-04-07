using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Validators/Placement Validator")]
public class PlacementValidator : ScriptableObject
{
	public MeshEntity selectionIndicator;
	public MeshEntity errorIndicator;

	public virtual bool ValidatePlacement(Map map, HexCoords pos, BuildingTileEntity buildingTile, IndicatorManager indicatorManager)
	{
		var tilesToOccupy = HexCoords.SpiralSelect(pos, buildingTile.size, innerRadius: map.innerRadius);
		bool outOfBounds = false;
		for (int i = 0; i < tilesToOccupy.Length; i++)
		{
			if(!tilesToOccupy[i].IsInBounds(map.totalHeight, map.totalWidth))
			{
				outOfBounds = true;
				break;
			}
		}
		if(outOfBounds)
		{
			for (int i = 0; i < tilesToOccupy.Length; i++)
			{
				var tile = map[tilesToOccupy[i]];
				if (tile != null)
					indicatorManager.SetIndicator(tile, errorIndicator);
			}
			return outOfBounds;
		}else
		{
			bool isValid = true;
			for (int i = 0; i < tilesToOccupy.Length; i++)
			{
				var tile = map[tilesToOccupy[i]];
				if (tile is BuildingTile || tile is ResourceTile)
				{
					indicatorManager.SetIndicator(tile, errorIndicator);
					isValid = false;
				}
				else
					indicatorManager.SetIndicator(tile, selectionIndicator);
			}
			return isValid;
		}
	}
}
