using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceDatabase : MonoBehaviour
{
	public ResourceList resourceList;
	#region Singleton
	public static ResourceDatabase INST
	{
		get
		{
			if (_instance == null)
				return _instance = GameObject.FindObjectOfType<ResourceDatabase>();
			return _instance;
		}
	}

	private static ResourceDatabase _instance;
	#endregion


	public static int ResourceCount => INST == null ? 0 : INST.resourceList.resourceDefinations.Length;

	public static int GetResourceId(string name)
	{
		for (int i = 0; i < INST.resourceList.resourceDefinations.Length; i++)
		{
			if (INST.resourceList.resourceDefinations[i].name == name)
				return i;
		}
		throw new System.Exception($"Resource '{name}' does not exits");
	}

	public static int GetResourceId(ResourceTileInfo tileInfo)
	{
		for (int i = 0; i < INST.resourceList.resourceDefinations.Length; i++)
		{
			if (INST.resourceList.resourceDefinations[i].resourceTile == tileInfo)
				return i;
		}
		throw new System.Exception($"Resource '{tileInfo.name}' does not exits");
	}

	public static string GetResourceName(int id, string locale = "en")
	{
		return INST.resourceList.resourceDefinations[id].name;
	}

	public static TileInfo GetResourceTile(int id)
	{
		return INST.resourceList.resourceDefinations[id].resourceTile;
	}

	public static int GetSpriteId(int id)
	{
		return INST.resourceList.resourceDefinations[id].spriteID;
	}

	public static Sprite GetSprite(int id)
	{
		return INST.resourceList.resourceDefinations[id].sprite;
	}

	public static int[] GetIdArray()
	{
		return Enumerable.Range(0, INST.resourceList.resourceDefinations.Length).ToArray();
	}

	public static string GetResourceString(string name) => GetResourceString(GetResourceId(name));

	public static string GetResourceString(int id, bool longVersion = false)
	{
		if(longVersion)
			return $"<sprite={GetSpriteId(id)}> {GetResourceName(id)}";
		else
			return $"<sprite={GetSpriteId(id)}>";
	}



	


}
