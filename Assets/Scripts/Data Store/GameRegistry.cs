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
	public static ResearchTreeUI ResearchTreeUI => INST.researchTreeUI;
	public static Camera Camera => INST.mainCamera;
	public static CameraController CameraController => INST.cameraController;
	public static BuildingDatabase BuildingDatabase => INST.buildingDatabase;
	public static ResearchDatabase ResearchDatabase => INST.researchDatabase;
	public static ResourceSystem ResourceSystem => INST.resourceSystem;
	public static ResearchSystem ResearchSystem => INST.researchSystem;
	public static TileDatabase TileDatabase => INST.tileDatabase;
	public static Map GameMap => INST.gameState.map;

	public StatusUI statusUI;
	public BaseNameWindowUI baseNameUI;
	public ResearchTreeUI researchTreeUI;
	public Camera mainCamera;
	public CameraController cameraController;
	public ResearchDatabase researchDatabase;
	public ResourceSystem resourceSystem;
	public ResearchSystem researchSystem;
	public TileDatabase tileDatabase;
	public GameState gameState;
	public BuildingDatabase buildingDatabase;


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

	public static void UnlockBuilding(BuildingIdentifier building)
	{
		if (!_inst.gameState.unlockedBuildings.Contains(building.id))
		{
			_inst.gameState.unlockedBuildings.Add(building.id);
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