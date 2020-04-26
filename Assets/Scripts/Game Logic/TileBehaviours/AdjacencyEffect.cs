using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Adjacency Effects/Basic")]
public class AdjacencyEffect : ScriptableObject, ISerializationCallbackReceiver
{
	public BonusDefination[] bonusDefinations;

	private Dictionary<BuildingIdentifier, ResourceIndentifier[]> _bonuses;

	public virtual List<string> GetAdjacencyEffectsString(BuildingTileEntity building, Tile[] neighbors)
	{
		var effects = new List<string>();
		for (int i = 0; i < neighbors.Length; i++)
		{
			if(neighbors[i] is BuildingTile b)
			{
				
			}
		}
		return effects;
	}

	public virtual void ApplyEffects(BuildingTileEntity building, Tile[] neighbors)
	{

	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		_bonuses = new Dictionary<BuildingIdentifier, ResourceIndentifier[]>();
		for (int i = 0; i < bonusDefinations.Length; i++)
			_bonuses.Add(bonusDefinations[i].building, bonusDefinations[i].bonuses);
	}
}

[Serializable]
public struct BonusDefination
{
	public BuildingIdentifier building;
	public ResourceIndentifier[] bonuses;
}