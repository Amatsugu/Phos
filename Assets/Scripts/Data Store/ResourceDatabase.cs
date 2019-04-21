using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceDatabase
{
	#region Singleton
	public static ResourceDatabase INST
	{
		get
		{
			if (_instance == null)
				return _instance = new ResourceDatabase();
			return _instance;
		}
	}

	private static ResourceDatabase _instance;
	#endregion

	public static int ResourceCount => (INST._resourceDefs == null) ? 0 : INST._resourceDefs.Length;

	private ResourceDefination[] _resourceDefs;

	public static int GetResourceId(string name)
	{
		for (int i = 0; i < INST._resourceDefs.Length; i++)
		{
			if (INST._resourceDefs[i].name == name)
				return i;
		}
		throw new System.Exception($"Resource '{name}' does not exits");
	}

	public static string GetResourceName(int id)
	{
		return id >= INST._resourceDefs.Length ? null : INST._resourceDefs[id].name;
	}

	public static TileInfo GetResourceTile(int id)
	{
		return id >= INST._resourceDefs.Length ? null : INST._resourceDefs[id].resourceTile;
	}

	public static int GetSpriteId(int id)
	{
		return id >= INST._resourceDefs.Length ? -1 : INST._resourceDefs[id].spriteID;
	}

	public static int[] GetIdArray()
	{
		return Enumerable.Range(0, INST._resourceDefs.Length).ToArray();
	}

	public static void Init(ResourceDefination[] resourceDefinations)
	{
		INST._resourceDefs = resourceDefinations;
	}


}
