using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UI/Unit Info")]
public class UnitInfo : ScriptableObject
{
	[SerializeField]
	public Resource[] cost;
	public Sprite icon;
	public string description;
	public BuildingTileInfo tile;

	[System.Serializable]
	public struct Resource
	{
		public string name;
		public int ammount;
	}

	public string GetProductionString()
	{
		var costString = "";
		for (int i = 0; i < tile.production.Length; i++)
		{
			costString += $"<size=.75em><voffset=.25em>+</voffset></size><sprite={ResourceDatabase.GetResourceId(tile.production[i].name)}> <size=.75em><voffset=.25em>{tile.production[i].ammount}/t</voffset></size>";
			if (i != tile.production.Length - 1)
				costString += "\n";
		}
		return costString;
	}

	public string GetCostString()
	{
		var costString = "";
		for (int i = 0; i < cost.Length; i++)
		{
			costString += $"<size=.75em><voffset=.25em>-</voffset></size><sprite={ResourceDatabase.GetResourceId(cost[i].name)}> <size=.75em><voffset=.25em>{cost[i].ammount}</voffset></size>";
			if (i != cost.Length - 1)
				costString += "\n";
		}
		if (tile.consumption.Length > 0)
			costString += "\n";
		for (int i = 0; i < tile.consumption.Length; i++)
		{
			costString += $"<size=.75em><voffset=.25em>-</voffset></size><sprite={ResourceDatabase.GetResourceId(tile.consumption[i].name)}> <size=.75em><voffset=.25em>{tile.consumption[i].ammount}/t</voffset></size>";
			if (i != tile.production.Length - 1)
				costString += "\n";
		}
		return costString;
	}
}