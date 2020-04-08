public class GameUI : UIHover
{
	private UIInfoPanel _infoPanel;
	private UIBuildPanel _buildPanel;
	private UIActionsPanel _actionsPanel;
	private UICategoryPanel _categoryPanel;

	private UIState state;

	private enum UIState
	{
		Disabled,
		Idle,
		PlaceBuilding,
		PlaceUnit,
		BuildingsSelected,
		UnitsSelected,
	}

	protected override void Awake()
	{
		base.Awake();
		_infoPanel = GetComponentInChildren<UIInfoPanel>();
		_buildPanel = GetComponentInChildren<UIBuildPanel>();
		_actionsPanel = GetComponentInChildren<UIActionsPanel>();
		_categoryPanel = GetComponentInChildren<UICategoryPanel>();

		_categoryPanel.OnButtonClicked += CategorySelected;
		_buildPanel.OnHide += OnBuildPanelClosed;

		SettupButtonCallbacks();

		GameEvents.OnGameReady += Init;
	}

	private void SettupButtonCallbacks()
	{
		_categoryPanel.tech.onClick.AddListener(() => CategorySelected(BuildingCategory.Tech));
		_categoryPanel.gathering.onClick.AddListener(() => CategorySelected(BuildingCategory.Gathering));
		_categoryPanel.production.onClick.AddListener(() => CategorySelected(BuildingCategory.Production));
		_categoryPanel.structure.onClick.AddListener(() => CategorySelected(BuildingCategory.Structure));
		_categoryPanel.military.onClick.AddListener(() => CategorySelected(BuildingCategory.Military));
		_categoryPanel.defense.onClick.AddListener(() => CategorySelected(BuildingCategory.Defense));
	}

	private void Init()
	{
		state = UIState.PlaceBuilding;
	}

	private void CategorySelected(BuildingCategory category)
	{
		state = UIState.PlaceBuilding;
		_buildPanel.Show(category);
	}

	private void OnBuildPanelClosed()
	{
		state = UIState.Idle;
	}
}