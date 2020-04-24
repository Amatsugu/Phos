using Unity.Mathematics;
using UnityEngine.UI;

public class UITabPanel : UIPanel
{
	public Button[] tabs;
	public bool autoSelectTab = false;
	[ConditionalHide("autoSelectTab")]
	public int selectedTab;

	private ColorBlock[] _baseColors;
	private ColorBlock[] _selectedColors;

#if DEBUG
	protected override void OnValidate()
	{
		base.OnValidate();
		selectedTab = math.clamp(selectedTab, 0, tabs.Length - 1);
	}
#endif

	protected override void Awake()
	{
		base.Awake();
		_baseColors = new ColorBlock[tabs.Length];
		_selectedColors = new ColorBlock[tabs.Length];
		for (int i = 0; i < tabs.Length; i++)
		{
			_baseColors[i] = tabs[i].colors;
			_selectedColors[i] = tabs[i].colors;
			_selectedColors[i].normalColor = _selectedColors[i].selectedColor;
			_selectedColors[i].highlightedColor = _selectedColors[i].selectedColor;
			int tab = i;
			tabs[i].onClick.AddListener(() => OnTabSelected(tab));
		}
		if (autoSelectTab)
			OnTabSelected(selectedTab);
	}

	protected virtual void OnTabSelected(int tab)
	{
		for (int i = 0; i < tabs.Length; i++)
		{
			if (i == tab)
				tabs[i].colors = _selectedColors[i];
			else
				tabs[i].colors = _baseColors[i];
		}
	}

	public virtual void DeselectAll()
	{
		for (int i = 0; i < tabs.Length; i++)
			tabs[i].colors = _baseColors[i];
	}
}