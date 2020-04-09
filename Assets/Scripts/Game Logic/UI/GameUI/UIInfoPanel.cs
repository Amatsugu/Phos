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
		titleText.SetText(building.info.name);
		descriptionText.SetText(building.info.description);
		productionText.SetText(building.info.GetProductionString());
		SetActive(true);
	}
}
