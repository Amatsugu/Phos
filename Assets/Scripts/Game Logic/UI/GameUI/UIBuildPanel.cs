using UnityEngine;
using UnityEngine.UI;

public class UIBuildPanel : UITabPanel
{
	public UIUnitIcon iconPrefab;
	public BuildingDatabase buildingDatabase;
	[HideInInspector]
	public UIInfoPanel infoPanel;


	public RectTransform contentArea;

	private UIUnitIcon[] _icons;
	private int _tier = 1;
	private BuildingCategory _lastCategory;

	protected override void Awake()
	{
		GameRegistry.SetBuildingDatabase(buildingDatabase);
		_icons = new UIUnitIcon[8];
		base.Awake();
	}

	protected override void OnTabSelected(int tab)
	{
		base.OnTabSelected(tab);
		_tier = tab + 1;
		Show(_lastCategory);
	}

	public void Show(BuildingCategory category)
	{
		_lastCategory = category;
		var buildings = buildingDatabase[category];
		for (int i = 0, j = 0; i < _icons.Length; i++)
		{
			if (_icons[i] == null)
			{
				_icons[i] = Instantiate(iconPrefab, contentArea, false);
				_icons[i].OnBlur += infoPanel.Hide;
			}
			if (j < buildings.Length)
			{
				if (buildings[j].info.tier == _tier)
				{
					_icons[i].SetActive(false);
					_icons[i].ClearHoverEvents();
					_icons[i].titleText.text = buildings[j].info.name;
					_icons[i].costText.text = buildings[j].info.GetCostString();
					_icons[i].icon.sprite = buildings[j].info.icon;
					var b = buildings[j];
					_icons[i].OnHover += () => infoPanel.ShowInfo(b);
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