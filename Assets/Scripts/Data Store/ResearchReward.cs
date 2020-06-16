using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Research Reward")]
public class ResearchReward : ScriptableObject
{
	public enum RewardType
	{
		Building,
		Unit,
		BuildingUpgrade,
		Custom
	}

	public RewardType type;
	public BuildingIdentifier building;
	public UnitIdentifier unit;
	public CustomResearchAction custom;

	public void ActivateReward()
	{
		switch (type)
		{
			case RewardType.Building:
				GameRegistry.UnlockBuilding(building);
				var buildingInfo = GameRegistry.BuildingDatabase[building].info;
				NotificationsUI.Notify(NotifType.Info, $"Unlocked Building: {buildingInfo.GetNameString()}", buildingInfo.description);
				break;

			case RewardType.Unit:
				break;

			case RewardType.BuildingUpgrade:
				GameRegistry.UnlockBuilding(building);
				var upgradeInfo = GameRegistry.BuildingDatabase[building].info;
				NotificationsUI.Notify(NotifType.Info, $"Unlocked Upgrade: {upgradeInfo.GetNameString()}", upgradeInfo.description);
				break;

			case RewardType.Custom:
				custom.Execute();
				break;
		}
	}

	public string GetRewardMessage()
	{
		switch (type)
		{
			case RewardType.Building:
				return $"Unlocks Building: {GameRegistry.BuildingDatabase[building].info.GetNameString()}";

			case RewardType.Unit:
				return "";

			case RewardType.BuildingUpgrade:
				return $"";

			case RewardType.Custom:
				return custom.GetRewardMessage();

			default:
				return null;
		}
	}
}

public abstract class CustomResearchAction : ScriptableObject
{
	public abstract void Execute();

	public abstract string GetRewardMessage();
}