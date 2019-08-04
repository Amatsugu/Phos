using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class StatusUI : MonoBehaviour
{
	public RectTransform resourcePanel;
	public GameObject resourceDisplayPrefab;
	public ResourceBreakdownUI resourceBreakdown;

	private UIResourceDisplay[] _displays;

	void Awake()
	{
		GameRegistry.INST.statusUI = this;
	}

	void Start()
	{
		_displays = new UIResourceDisplay[ResourceDatabase.ResourceCount];
		var w = resourceDisplayPrefab.GetComponent<RectTransform>().rect.width;
		for (int i = 0; i < _displays.Length; i++)
		{
			var rDisp = Instantiate(resourceDisplayPrefab, Vector3.zero, Quaternion.identity, resourcePanel).GetComponent<RectTransform>();
			//rDisp.anchoredPosition = new Vector3(i * w, 0, 0);
			_displays[i] = rDisp.GetComponent<UIResourceDisplay>();
			_displays[i].SetIcon(ResourceDatabase.GetSprite(i));
			rDisp.gameObject.SetActive(false);
			var id = i;
			_displays[i].OnHover += () =>
			{
				resourceBreakdown.gameObject.SetActive(true);
				resourceBreakdown.SetResource(id);
				var pos = resourceBreakdown.rTransform.anchoredPosition;
				pos.x = _displays[id].GetComponent<RectTransform>().position.x;
				resourceBreakdown.rTransform.anchoredPosition = pos;
			};
			_displays[i].OnBlur += () =>
			{
				resourceBreakdown.gameObject.SetActive(false);
			};
		}
	}

	void Update()
	{
		for (int i = 0; i < _displays.Length; i++)
		{
			if(ResourceSystem.resCount[i] != 0)
			{
				_displays[i].gameObject.SetActive(true);
			}
			var transactions = GameRegistry.ResourceSystem.resourceRecords[i];

			_displays[i].SetInfo(ResourceSystem.resCount[i],
								transactions.totalProduction,
								Mathf.Abs(transactions.totalDemand),
								transactions.totalProduction - transactions.totalSatisfaction,
								ResourceSystem.resCount[i] == ResourceSystem.maxStorage);
		}
	}

}
