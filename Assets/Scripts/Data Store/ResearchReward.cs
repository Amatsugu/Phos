using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Research Reward")]
public class ResearchReward : ScriptableObject
{
	[System.Serializable]
	public enum RewardType
	{
		Building,
		Unit,
		Upgrade,
		Custom
	}

	public RewardType type;
	public BuildingIdentifier building;

	
}


[System.Serializable]
public struct BuildingIdentifier
{
	public BuildingTileInfo building;
}
