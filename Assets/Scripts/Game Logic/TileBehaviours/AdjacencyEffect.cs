using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Adjacency Effects/Basic")]
[Serializable]
public class AdjacencyEffect : ScriptableObject, ISerializationCallbackReceiver
{
	public BonusDefination[] bonusDefinations;

	private Dictionary<int, BonusDefination> _bonuses;

	public virtual void GetAdjacencyEffectsString(BuildingTileEntity building, Tile[] neighbors, ref List<string> effectsString)
	{
		var (prod, cons) = CalculateBonuses(neighbors);
		effectsString.Add($"+{cons}x Production Rate");
		effectsString.Add($"-{prod}x Consumption Rate");
	}

	public virtual void ApplyEffects(BuildingTile building, Tile[] neighbors)
	{
		var (prod, cons) = CalculateBonuses(neighbors);
		var e = building.GetBuildingEntity();
		if (prod > 0)
		{
		}
	}

	private (float prodBonus, float consBonus) CalculateBonuses(Tile[] neighbors)
	{
		var productionBonus = 0f;
		var consumtionBonus = 0f;
		for (int i = 0; i < neighbors.Length; i++)
		{
			if (neighbors[i] is BuildingTile b)
			{
				Debug.Log(b.info);
				var bonusID = b.info.GetInstanceID();
				if (_bonuses.ContainsKey(bonusID))
				{
					var bonus = _bonuses[bonusID];
					productionBonus += bonus.productionMultiplier;
					consumtionBonus += bonus.consumptionMultiplier;
				}
			}
		}
		return (productionBonus, consumtionBonus);
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