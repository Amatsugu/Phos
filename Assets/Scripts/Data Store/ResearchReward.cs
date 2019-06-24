using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResearchReward
{
	public enum RewardType
	{
		Building,
		Unit,
		Upgrade,
		Custom
	}

	
}


public struct BuildingIdentifier
{
	public BuildingTileInfo building;
}
