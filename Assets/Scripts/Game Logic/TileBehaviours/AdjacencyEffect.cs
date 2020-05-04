using Effects.Lines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Adjacency Effects/Basic")]
[Serializable]
public class AdjacencyEffect : ScriptableObject
{
	public BonusDefination[] bonusDefinations;
	public MeshEntityRotatable line;

	public virtual void GetAdjacencyEffectsString(Tile tile, Tile[] neighbors, ref List<string> effectsString)
	{
		var (prod, cons) = CalculateBonuses(neighbors, tile);
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

	private (float prodBonus, float consBonus) CalculateBonuses(Tile[] neighbors, Tile building = null)
	{
		var pos = new List<float3>();
		var height = 0f;
		var productionBonus = 0f;
		var consumtionBonus = 0f;
		for (int i = 0; i < neighbors.Length; i++)
		{
			if (neighbors[i] is BuildingTile b)
			{
				var bId = GameRegistry.TileDatabase.entityIds[b.info];
				for (int j = 0; j < bonusDefinations.Length; j++)
				{
					if(bonusDefinations[j].buildingsSet.Contains(bId))
					{
						var bonus = bonusDefinations[j];
						pos.Add(b.SurfacePoint);
						if (b.Height > height)
							height = b.Height;
						productionBonus += bonus.productionMultiplier;
						consumtionBonus += bonus.consumptionMultiplier;
					}

				}
			}
		}
		if (building != null)
		{
			for (int i = 0; i < pos.Count; i++)
			{
				RenderLine(building.SurfacePoint, pos[i], height);
			}
		}
		return (productionBonus, consumtionBonus);
	}

	private void RenderLine(float3 a, float3 b, float height)
	{
		var a2 = new float3(a.x, height + 2, a.z);
		var b2 = new float3(b.x, height + 2, b.z);
		var e1 = LineFactory.CreateStaticLine(line, a, a2, 0.05f);
		var e2 = LineFactory.CreateStaticLine(line, a2, b2, 0.05f);
		var e3 = LineFactory.CreateStaticLine(line, b2, b, 0.05f);
		Map.EM.AddComponentData(e1, new DeathTime { Value = Time.time });
		Map.EM.AddComponentData(e2, new DeathTime { Value = Time.time });
		Map.EM.AddComponentData(e3, new DeathTime { Value = Time.time });
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