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
		descriptionText.SetText($"{building.info.description}\nUpkeep: {building.info.GetUpkeepString()}");
		productionText.SetText(building.info.GetProductionString());
		SetActive(true);
	}
}
