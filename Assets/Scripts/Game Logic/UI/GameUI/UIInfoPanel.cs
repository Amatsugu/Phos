using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIInfoPanel : UIPanel
{
	public TMP_Text descriptionText;
	public TMP_Text productionText;

	public void ShowInfo(BuildingDatabase.BuildingDefination building)
	{
		titleText.SetText(building.info.GetNameString());
		var upkeep = building.info.GetUpkeepString();
		descriptionText.SetText($"{building.info.description}\n{(upkeep.Length > 0 ? $"Upkeep: {upkeep}" : "")}");
		productionText.SetText(building.info.GetProductionString());
		SetActive(true);
	}

	public void ShowInfo(MobileUnitEntity unitEntity)
	{
		titleText.SetText(unitEntity.GetNameString());

		SetActive(true);
	}
}
