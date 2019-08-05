using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceBreakdownUI : UIHover
{
	[Header("Title Text")]
	public RectTransform productionTitle;
	public RectTransform demandTitle;
	public RectTransform satisfactionTitle;
	public RectTransform excessTitle;
	[Header("Body Text")]
	public TMP_Text productionText;
	public TMP_Text demandText;
	public TMP_Text satisfactionText;
	public TMP_Text excessText;


	private int _resId;
	private float _targetHeight;
	private float _animTime;

	protected override void Awake()
	{
		base.Awake();
		gameObject.SetActive(false);
	}


	protected override void LateUpdate()
	{
		base.LateUpdate();
		productionText.SetText(GetProductionText(_resId));
		demandText.SetText(GetDemandText(_resId));
		satisfactionText.SetText(GetSatisfactionText(_resId));
		excessText.SetText(GetExcessText(_resId));
		var prefHeight = 0f;
		prefHeight += productionTitle.rect.height;
		prefHeight += demandTitle.rect.height;
		prefHeight += satisfactionTitle.rect.height;
		prefHeight += excessTitle.rect.height;

		prefHeight += productionText.preferredHeight;
		prefHeight += demandText.preferredHeight;
		prefHeight += satisfactionText.preferredHeight;
		prefHeight += excessText.preferredHeight;

		prefHeight += 5 * 8;
		
		if(_targetHeight != prefHeight && prefHeight != rTransform.rect.height)
		{
			_animTime = 0;
			_targetHeight = prefHeight;
		}
		_animTime += Time.deltaTime * 2;

		rTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Lerp(rTransform.rect.height, _targetHeight, _animTime));

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
