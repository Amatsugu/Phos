using System.Collections;
using System.Collections.Generic;
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
		switch(type)
		{
			case RewardType.Building:
				GameRegistry.UnlockBuilding(building);
				break;
			case RewardType.Unit:
				//TODO: Implement unlocking unit
				break;
			case RewardType.BuildingUpgrade:
				GameRegistry.UnlockBuilding(building);
				break;
			case RewardType.Custom:
				custom.Execute();
				break;
		}
	}
}

public abstract class CustomResearchAction : ScriptableObject
{
	public abstract void Execute();
}


[System.Serializable]
public class BuildingIdentifier
{
	public int id = -1;
}

[System.Serializable]
public struct UnitIdentifier
{
	public MobileUnitInfo unit;
}
