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
		Upgrade,
		Custom
	}

	public RewardType type;
	public BuildingIdentifier building;
	public UnitIdentifier unit;

	
}


[System.Serializable]
public struct BuildingIdentifier
{
	public int buildingId;
}

[System.Serializable]
public struct UnitIdentifier
{
	public MobileUnitInfo unit;
}
