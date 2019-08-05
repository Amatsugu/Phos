using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class StatusUI : UIHover
{
	public RectTransform resourcePanel;
	public GameObject resourceDisplayPrefab;
	public ResourceBreakdownUI resourceBreakdown;
	public float animSpeed = 2;

	private UIResourceDisplay[] _displays;
	private int _selectedDisplay = -1;
	private float _animTime = 0;

	protected override void Awake()
	{
		base.Awake();
		GameRegistry.INST.statusUI = this;
	}

	protected override void Start()
	{
		base.Start();
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
				_selectedDisplay = id;
				if(!resourceBreakdown.IsActive)
				{
					var targetX = _displays[_selectedDisplay].rTransform.position.x;
					var pos = resourceBreakdown.rTransform.anchoredPosition;
					pos.x = targetX;
					resourceBreakdown.rTransform.anchoredPosition = pos;
				}
				resourceBreakdown.SetActive(true);
				resourceBreakdown.SetResource(id);
				_animTime = 0;
			};
		}
		OnBlur += () =>
		{
			_selectedDisplay = -1;
			resourceBreakdown.SetActive(false);
		};
		resourceBreakdown.SetActive(false);
	}

	protected override void Update()
	{
		base.Update();
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

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if(_selectedDisplay != -1)
		{
			_animTime += Time.deltaTime * animSpeed;
			var targetX = _displays[_selectedDisplay].rTransform.position.x;
			var pos = resourceBreakdown.rTransform.anchoredPosition;
			pos.x = Mathf.Lerp(pos.x, targetX, _animTime);
			resourceBreakdown.rTransform.anchoredPosition = pos;
		}
	}

}
