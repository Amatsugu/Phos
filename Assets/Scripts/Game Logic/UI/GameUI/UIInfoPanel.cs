using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Units;

using System.Collections;
using System.Collections.Generic;
using System.Text;

using TMPro;
using UnityEngine;

public class UIInfoPanel : UIPanel
{
	public TMP_Text descriptionText;
	public TMP_Text productionText;

	public void ShowInfo(BuildingTileEntity building)
	{
		titleText.SetText(building.GetNameString());
		var upkeep = building.GetUpkeepString();
		descriptionText.SetText($"{building.description}\n{(upkeep.Length > 0 ? $"Upkeep: {upkeep}" : "")}");
		productionText.SetText(building.GetProductionString());
		SetActive(true);
	}

	public void ShowInfo(ResourceTileInfo tile)
	{
		titleText.SetText(tile.GetNameString());
		descriptionText.SetText(tile.description);
		var sb = new StringBuilder();
		for (int i = 0; i < tile.resourceYields.Length; i++)
		{
			sb.Append($"{ResourceDatabase.GetResourceName(tile.resourceYields[i].id)}");
		}
		productionText.SetText(sb);
		SetActive(true);
	}

	public void ShowInfo(MobileUnitEntity unitEntity)
	{
		titleText.SetText(unitEntity.GetNameString());

		SetActive(true);
	}
}
