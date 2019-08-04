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

	public RectTransform rTransform;

	private int _resId;

	void Awake()
	{
		rTransform = GetComponent<RectTransform>();
		gameObject.SetActive(false);
	}


    void LateUpdate()
    {
		productionText.SetText(GetProductionText(_resId));
		demandText.SetText(GetDemandText(_resId));
		satisfactionText.SetText(GetSatisfactionText(_resId));
		excessText.SetText(GetExcessText(_resId));
	}

	public void SetResource(int resId)
	{
		_resId = resId;
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
			text += $"+{ResourceDatabase.GetResourceString(resId)}{info.production}/t \t{info.productionCount}x {buildingName}\n";
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
			text += $"-{ResourceDatabase.GetResourceString(resId)}{info.demand}/t \t{info.demandCount}x {buildingName}\n";
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
			text += $"{ResourceDatabase.GetResourceString(resId)}{info.satisfaction}/t \t{info.satisfactionCount}x {buildingName}\n";
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
			text += $"{ResourceDatabase.GetResourceString(resId)}{info.excess}/t \t{info.excessCount}x {buildingName}\n";
		}
		return text;
	}
}
