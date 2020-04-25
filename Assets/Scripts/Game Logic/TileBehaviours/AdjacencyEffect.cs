using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Adjacency Effects/Basic")]
public class AdjacencyEffects : ScriptableObject
{
	public virtual List<string> GetAdjacencyEffectsString(BuildingTileEntity building, Tile[] neighbors)
	{
		var effects = new List<string>();

		return effects;
	}

	public virtual void ApplyEffects(BuildingTileEntity building, Tile[] neighbors)
	{

	}
}