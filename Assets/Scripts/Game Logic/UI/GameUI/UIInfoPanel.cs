using Amatsugu.Phos.TileEntities;

using System.Collections;
using System.Collections.Generic;
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

	public void ShowInfo(MobileUnitEntity unitEntity)
	{
		titleText.SetText(unitEntity.GetNameString());

		SetActive(true);
	}
}
