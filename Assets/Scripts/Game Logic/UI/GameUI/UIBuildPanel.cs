using UnityEngine;
using UnityEngine.UI;

public class UIBuildPanel : UIPanel
{
	public UIUnitIcon iconPrefab;
	public BuildingDatabase buildingDatabase;

	public Button[] tierTabs;

	public RectTransform contentArea;

	private UIUnitIcon[] _icons;
	private int _tier = 1;
	private BuildingCategory _lastCategory;

	protected override void Awake()
	{
		GameRegistry.SetBuildingDatabase(buildingDatabase);
		_icons = new UIUnitIcon[12];
		for (int i = 0; i < tierTabs.Length; i++)
		{
			var tier = i + 1;
			tierTabs[i].onClick.AddListener(() =>
			{
				_tier = tier;
				Show(_lastCategory);
			});
		}
		base.Awake();
	}

	public void Show(BuildingCategory category)
	{
		_lastCategory = category;
		var buildings = buildingDatabase[category];
		for (int i = 0, j = 0; i < _icons.Length; i++)
		{
			if (_icons[i] == null)
				_icons[i] = Instantiate(iconPrefab, contentArea, false);
			if (j < buildings.Length)
			{
				if (buildings[j].info.tier == _tier)
				{
					_icons[i].SetActive(false);
					_icons[i].text.text = buildings[j].info.name;
					_icons[i].icon.sprite = buildings[j].info.icon;
					_icons[i].SetActive(true);
				}
				else
					i--;
				j++;
			}else
				_icons[i].SetActive(false);
		}
		Show();
	}
}