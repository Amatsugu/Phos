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
				GameRegistry.BuildingDatabase.buildings[building.id].isUnlocked = true;
				break;
			case RewardType.Unit:
				//TODO: Implement unlocking unit
				break;
			case RewardType.BuildingUpgrade:
				GameRegistry.BuildingDatabase.buildings[building.id].isUnlocked = true;
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
public struct BuildingIdentifier
{
	public int id;
}

[System.Serializable]
public struct UnitIdentifier
{
	public MobileUnitInfo unit;
}
