using Effects.Lines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
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
		var (prod, cons) = CalculateBonuses(neighbors, out var selection);
		for (int i = 0; i < selection.Count; i++)
			RenderPreviewLine(tile.SurfacePoint, selection[i].SurfacePoint, tile.Height);
		effectsString.Add($"+{prod}x Production Rate");
		effectsString.Add($"-{cons}x Consumption Rate");
	}

	public virtual NativeArray<Entity> ApplyEffects(BuildingTile building, Tile[] neighbors)
	{
		var (prod, cons) = CalculateBonuses(neighbors, out var selection);
		var e = building.GetBuildingEntity();
		var connectors = new NativeArray<Entity>(selection.Count * 3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		building.AddProductionMulti(prod);
		building.AddConsumptionMulti(cons);
		for (int i = 0; i < selection.Count; i++)
		{
			RenderConnectionLine(building.SurfacePoint, selection[i].SurfacePoint, connectors.Slice(i * 3, 3));
		}
		return connectors;

	}

	private (float prodBonus, float consBonus) CalculateBonuses(Tile[] neighbors, out List<BuildingTile> buildings)
	{
		var productionBonus = 0f;
		var consumtionBonus = 0f;
		buildings = new List<BuildingTile>();
		for (int i = 0; i < neighbors.Length; i++)
		{
			if (neighbors[i] is BuildingTile b)
			{
				var bId = GameRegistry.TileDatabase.entityIds[b.info];
				for (int j = 0; j < bonusDefinations.Length; j++)
				{
					if (bonusDefinations[j].buildingsSet.Contains(bId))
					{
						var bonus = bonusDefinations[j];
						buildings.Add(b);
						productionBonus += bonus.productionMultiplier;
						consumtionBonus += bonus.consumptionMultiplier;
					}

				}
			}
		}
		return (productionBonus, consumtionBonus);
	}

	private void RenderConnectionLine(float3 a, float3 b, NativeSlice<Entity> entities)
	{
		a.y += 0.01f;
		b.y += 0.01f;
		var dir = new float3(b.x, a.y, b.z) - a;
		var a2 = a + (dir *.5f);
		var b2 = new float3(a2.x, b.y, a2.z);
		if (a.y < b.y)
			a2 -= dir * 0.01f;
		else if(a.y > b.y)
			a2 += dir * 0.01f;
		entities[0] = LineFactory.CreateStaticLine(line, a, a2, 0.05f);
		entities[1] = LineFactory.CreateStaticLine(line, a2, b2, 0.05f);
		entities[2] = LineFactory.CreateStaticLine(line, b2, b, 0.05f);
	}

	private void RenderPreviewLine(float3 a, float3 b, float height)
	{
		var a2 = new float3(a.x, height + 2, a.z);
		var b2 = new float3(b.x, height + 2, b.z);
		var e1 = LineFactory.CreateStaticLine(line, a, a2, 0.05f);
		var e2 = LineFactory.CreateStaticLine(line, a2, b2, 0.05f);
		var e3 = LineFactory.CreateStaticLine(line, b2, b, 0.05f);
		Map.EM.AddComponentData(e1, new DeathTime { Value = Time.time + .01f });
		Map.EM.AddComponentData(e2, new DeathTime { Value = Time.time + .01f });
		Map.EM.AddComponentData(e3, new DeathTime { Value = Time.time + .01f });
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