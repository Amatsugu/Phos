using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Adjacency Effects/Basic")]
[Serializable]
public class AdjacencyEffect : ScriptableObject
{
	public BonusDefination[] bonusDefinations;

	public virtual void GetAdjacencyEffectsString(BuildingTileEntity building, Tile[] neighbors, ref List<string> effectsString)
	{
		var (prod, cons) = CalculateBonuses(neighbors);
		effectsString.Add($"+{prod}x Production Rate");
		effectsString.Add($"-{cons}x Consumption Rate");
	}

	public virtual void ApplyEffects(BuildingTile building, Tile[] neighbors)
	{
		var (prod, cons) = CalculateBonuses(neighbors);
		var e = building.GetBuildingEntity();
		var prodMulti = Map.EM.GetComponentData<ProductionMulti>(e);
		var consMulti = Map.EM.GetComponentData<ConsumptionMulti>(e);
		prodMulti.Value += prod;
		consMulti.Value += cons;
		Map.EM.SetComponentData(e, prodMulti);
		Map.EM.SetComponentData(e, consMulti);
	}

	private (float prodBonus, float consBonus) CalculateBonuses(Tile[] neighbors)
	{
		var productionBonus = 0f;
		var consumtionBonus = 0f;
		for (int i = 0; i < neighbors.Length; i++)
		{
			if (neighbors[i] is BuildingTile b)
			{
				var building = b.info.GetInstanceID();
				for (int j = 0; j < bonusDefinations.Length; j++)
				{
					if(bonusDefinations[j].buildingsSet.Contains(building))
					{
						var bonus = bonusDefinations[j];
						productionBonus += bonus.productionMultiplier;
						consumtionBonus += bonus.consumptionMultiplier;
					}

				}
			}
		}
		return (productionBonus, consumtionBonus);
	}
}

[Serializable]
public struct BonusDefination : ISerializationCallbackReceiver
{
	public BuildingIdentifier[] buildings;
	public HashSet<int> buildingsSet;
	public float productionMultiplier;
	public float consumptionMultiplier;

	public void OnAfterDeserialize()
	{
		if (buildings == null)
			return;
		buildingsSet = new HashSet<int>();
		for (int i = 0; i < buildings.Length; i++)
			buildingsSet.Add(buildings[i].id);
	}

	public void OnBeforeSerialize()
	{
	}
}