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

	private UIResourceDisplay[] _displays;

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
		}
		resourcePanel.GetComponent<UIStack>().UpdateChildren();
	}

	void Update()
	{
		for (int i = 0; i < _displays.Length; i++)
		{
			if(ResourceSystem.resCount[i] != 0)
			{
				_displays[i].gameObject.SetActive(true);
			}
			_displays[i].SetInfo(ResourceSystem.resCount[i],
								ResourceSystem.totalProduction[i],
								Mathf.Abs(ResourceSystem.totalDemand[i]),
								ResourceSystem.totalProduction[i] + ResourceSystem.totalDemand[i],
								ResourceSystem.resCount[i] == ResourceSystem.maxStorage);
		}
	}

}
