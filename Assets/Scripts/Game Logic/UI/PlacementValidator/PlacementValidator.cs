using Amatsugu.Phos;
using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Tiles;

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(menuName = "Validators/Placement Validator")]
[Serializable]
public class PlacementValidator : ScriptableObject
{
	public MeshEntity selectionIndicator;
	public MeshEntity errorIndicator;

	public virtual bool ValidatePlacement(Map map, HexCoords pos, BuildingTileEntity buildingTile, IndicatorManager indicatorManager, int rotation)
	{
		var tilesToOccupy = buildingTile.footprint.GetOccupiedTiles(pos, rotation);

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
			indicatorManager.LogError("Out of map bounds (how?)");
			return outOfBounds;
		}else
		{
			bool isValid = true;
			for (int i = 0; i < tilesToOccupy.Length; i++)
			{
				var tile = map[tilesToOccupy[i]];
				if (buildingTile.isOffshore && buildingTile.offshoreOnly)
				{
					if (!tile.IsUnderwater || tile is BuildingTile)
					{
						indicatorManager.SetIndicator(tile, errorIndicator);
						isValid = false;
					}
					else
						indicatorManager.SetIndicator(tile, selectionIndicator);
				} else
				{
					if (tile is BuildingTile)
					{
						indicatorManager.SetIndicator(tile, errorIndicator);
						isValid = false;
					}
					else if(tile is ResourceTile && !(tile.IsUnderwater && buildingTile.isOffshore))
					{
						indicatorManager.SetIndicator(tile, errorIndicator);
						isValid = false;
					}
					else
						indicatorManager.SetIndicator(tile, selectionIndicator);
				}
			}
			for (int i = 0; i < buildingTile.cost.Length; i++)
			{
				if (!ResourceSystem.HasResource(buildingTile.cost[i]))
				{
					indicatorManager.LogError("Insuffient resources");
					for (int j = 0; j < tilesToOccupy.Length; j++)
						indicatorManager.SetIndicator(map[tilesToOccupy[j]], errorIndicator);
					return false;
				}
			}

			var neighbors = buildingTile.footprint.GetNeighbors(pos, rotation);
			for (int i = 0; i < neighbors.Count; i++)
			{
				indicatorManager.SetIndicator(map[neighbors[i]], errorIndicator);
			}
			if (!isValid)
				indicatorManager.LogError("Cannot place on these tiles");
			return isValid;
		}
	}
}
