using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceBreakdownUI : MonoBehaviour
{
	public TMP_Text productionText;
	public TMP_Text demandText;
	public TMP_Text satisfactionText;
	public TMP_Text excessText;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		productionText.SetText(GetProductionText(0));
		demandText.SetText(GetDemandText(0));
		satisfactionText.SetText(GetSatisfactionText(0));
		excessText.SetText(GetExcessText(0));
	}

	string GetProductionText(int resId)
	{
		var transaction = GameRegistry.ResourceSystem.resourceRecords[resId];
		string text = "";
		foreach (var building in transaction.transactions)
		{
			var info = building.Value;
			if (info.production == 0)
				continue;
			var buildingName = GameRegistry.BuildingDatabase.buildings[building.Key].info.name;
			text += $"+{ResourceDatabase.GetResourceString(resId)}{info.production}/t \t{buildingName}\n";
		}
		return text;
	}

	string GetDemandText(int resId)
	{
		var transaction = GameRegistry.ResourceSystem.resourceRecords[resId];
		string text = "";
		foreach (var building in transaction.transactions)
		{
			var info = building.Value;
			if (info.demand == 0)
				continue;
			var buildingName = GameRegistry.BuildingDatabase.buildings[building.Key].info.name;
			text += $"-{ResourceDatabase.GetResourceString(resId)}{info.demand}/t \t{buildingName}\n";
		}
		return text;
	}

	string GetSatisfactionText(int resId)
	{
		var transaction = GameRegistry.ResourceSystem.resourceRecords[resId];
		string text = "";
		foreach (var building in transaction.transactions)
		{
			var info = building.Value;
			if (info.satisfaction == 0)
				continue;
			var buildingName = GameRegistry.BuildingDatabase.buildings[building.Key].info.name;
			text += $"{ResourceDatabase.GetResourceString(resId)}{info.satisfaction}/t \t{buildingName}\n";
		}
		return text;
	}

	string GetExcessText(int resId)
	{
		var transaction = GameRegistry.ResourceSystem.resourceRecords[resId];
		string text = "";
		foreach (var building in transaction.transactions)
		{
			var info = building.Value;
			if (info.excess == 0)
				continue;
			var buildingName = GameRegistry.BuildingDatabase.buildings[building.Key].info.name;
			text += $"{ResourceDatabase.GetResourceString(resId)}{info.excess}/t \t{buildingName}\n";
		}
		return text;
	}
}
