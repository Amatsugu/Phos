using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

public class GameRegistry : MonoBehaviour
{
	public static GameRegistry INST
	{
		get
		{
			if (_inst == null)
				_inst = FindObjectOfType<GameRegistry>();
			return _inst;
		}
	}

	private static GameRegistry _inst;

	public static GameState GameState => INST.gameState;
	public static StatusUI StatusUI => INST.statusUI;
	public static BaseNameWindowUI BaseNameUI => INST.baseNameUI;
	public static Camera Camera => INST.mainCamera;

	internal static void SetBaseName(string name)
	{
		INST.gameState.baseName = name;
	}

	public static CameraController CameraController => INST.cameraController;
	public static BuildingDatabase BuildingDatabase => INST.buildingDatabase;
	public static ResourceSystem ResourceSystem => INST.resourceSystem;
	public static TileDatabase TileDatabase => INST.tileDatabase;
	public static UnitDatabase UnitDatabase => INST.unitDatabase;
	public static ProjectileDatabase ProjectileDatabase => INST.projectileDatabase;
	public static Map GameMap => INST.gameState.map;
	public static RarityColors RarityColors => INST.rarityColors;

	internal static void SetState(GameState gameState)
	{
		INST.gameState = gameState;
		ResourceSystem.resCount = gameState.resCount;
	}


	public StatusUI statusUI;
	public BaseNameWindowUI baseNameUI;
	public Camera mainCamera;
	public CameraController cameraController;
	public ResourceSystem resourceSystem;
	public GameState gameState;
	//Databases
	public RarityColors rarityColors;
	public TileDatabase tileDatabase;
	public UnitDatabase unitDatabase;
	public BuildingDatabase buildingDatabase;
	public ProjectileDatabase projectileDatabase;


	public static void InitGame(GameState gameState)
	{
		INST.gameState = gameState;
		ResourceSystem.resCount = gameState.resCount;
	}

	public static void InitGame(Map map)
	{
		INST.gameState = new GameState(map);
		var buildings = INST.buildingDatabase.buildings.Values.ToArray();
		for (int i = 0; i < INST.buildingDatabase.buildings.Count; i++)
		{
			if (buildings[i].info.tier == 1)
			{
				INST.gameState.unlockedBuildings.Add(buildings[i].id);
			}
		}
	}

	public static void UnlockBuilding(BuildingIdentifier building, bool notify = true)
	{
		if (!_inst.gameState.unlockedBuildings.Contains(building.id))
		{
			_inst.gameState.unlockedBuildings.Add(building.id);
			if(notify)
				GameEvents.InvokeOnBuildingUnlocked(BuildingDatabase[building].info);
		}
	}

	public static bool IsBuildingUnlocked(int id)
	{
		return _inst.gameState.unlockedBuildings.Contains(id);
	}

	public static class Cheats
	{
		public static bool INSTANT_RESEARCH;
		public static bool INSTANT_BUILD;
		public static bool NO_RESOURCE_COST;
	}
}