using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Adjacency Effects/Basic")]
public class AdjacencyEffect : ScriptableObject, ISerializationCallbackReceiver
{
	public BonusDefination[] bonusDefinations;

	private Dictionary<int, BonusDefination> _bonuses;

	public virtual void GetAdjacencyEffectsString(BuildingTileEntity building, Tile[] neighbors, ref List<string> effectsString)
	{
		var productionBonus = 0f;
		var consumtionBonus = 0f;
		for (int i = 0; i < neighbors.Length; i++)
		{
			if(neighbors[i] is BuildingTile b)
			{
				var bonusID = b.info.GetInstanceID();
				if (_bonuses.ContainsKey(bonusID))
				{
					var bonus = _bonuses[bonusID];
					productionBonus += bonus.productionMultiplier;
					consumtionBonus += bonus.consumptionMultiplier;
				}
			}
		}
		effectsString.Add($"+{productionBonus}x Production Rate");
		effectsString.Add($"-{consumtionBonus}x Consumption Rate");
	}

	public virtual void ApplyEffects(BuildingTileEntity building, Tile[] neighbors)
	{

	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		_bonuses = new Dictionary<int, BonusDefination>();
		for (int i = 0; i < bonusDefinations.Length; i++)
			_bonuses.Add(bonusDefinations[i].building.id, bonusDefinations[i]);
	}
}

[Serializable]
public struct BonusDefination
{
	public BuildingIdentifier building;
	public float productionMultiplier;
	public float consumptionMultiplier;
}