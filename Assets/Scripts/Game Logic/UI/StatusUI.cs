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
	public float padding = 5;

	private UIResourceDisplay[] _displays;

	void Start()
	{
		_displays = new UIResourceDisplay[ResourceDatabase.ResourceCount];
		//TODO: retrieve resource id list from resource database
		var w = resourceDisplayPrefab.GetComponent<RectTransform>().rect.width + padding;
		for (int i = 0; i < _displays.Length; i++)
		{
			var rDisp = Instantiate(resourceDisplayPrefab, Vector3.zero, Quaternion.identity, resourcePanel).GetComponent<RectTransform>();
			rDisp.anchoredPosition = new Vector3(i * w, 0, 0);
			_displays[i] = rDisp.GetComponent<UIResourceDisplay>();
			/// TODO: implement get sprite method
			_displays[i].SetIcon(ResourceDatabase.GetSprite(i));
		}
		resourcePanel.GetComponent<UIStack>().UpdateChildren();
	}

	void Update()
	{
		if (ResourceSystem.resCount.Length == 0) //TODO: Resolve Resourse Database creation
			return;
		for (int i = 0; i < _displays.Length; i++)
		{
			if(ResourceSystem.resCount[i] == 0)
			{
				_displays[i].gameObject.SetActive(false);
				continue;
			}else
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
