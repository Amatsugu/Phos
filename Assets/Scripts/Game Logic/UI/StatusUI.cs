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
	public float minWidth = 164f;

	private UIResourceDisplay[] _displays;

	void Start()
	{
		_displays = new UIResourceDisplay[ResourceDatabase.ResourceCount];
		var w = Mathf.Max(minWidth, resourcePanel.rect.width/_displays.Length);
		Debug.Log(resourcePanel.rect.width/_displays.Length);
		//TODO: retrieve resource id list from resource database
		for (int i = 0; i < _displays.Length; i++)
		{
			var rDisp = Instantiate(resourceDisplayPrefab, new Vector3(i * w, 0, 0), Quaternion.identity, resourcePanel).GetComponent<RectTransform>();
			rDisp.anchoredPosition = new Vector3(i * w, 0, 0);
			_displays[i] = rDisp.GetComponent<UIResourceDisplay>();
			_displays[i].SetWidth(w);
			/// TODO: implement get sprite method
			//_displays[i].SetIcon(ResourceDatabase.get)
		}
	}

	void Update()
	{
		if (ResourceSystem.resCount.Length == 0) //TODO: Resolve Resourse Database creation
			return;
		for (int i = 0; i < _displays.Length; i++)
		{
			_displays[i].SetInfo(ResourceSystem.resCount[i],
								ResourceSystem.totalProduction[i],
								Mathf.Abs(ResourceSystem.totalDemand[i]),
								ResourceSystem.totalProduction[i] + ResourceSystem.totalDemand[i],
								ResourceSystem.resCount[i] == ResourceSystem.maxStorage);
		}
	}

}
