using Amatsugu.Phos.DataStore;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Profiling;

namespace Amatsugu.Phos.UI
{
	public class QuickSave : MonoBehaviour
	{
		private GameSave gameSave;

		private bool canSave = false;

		public void Start()
		{
			GameEvents.OnHQPlaced += OnHQ;
		}

		void OnHQ()
		{
			GameEvents.OnHQPlaced -= OnHQ;
			canSave = true;
		}

		public void Update()
		{
			if (!canSave)
				return;
			if (GameRegistry.BaseNameUI.panel.IsOpen)
				return;
			//if (Input.GetKeyUp(KeyCode.F6))
			//	Save();
			//if (Input.GetKeyUp(KeyCode.F7))
			//	Load();
		}

		void Save()
		{
			Debug.Log("Quick Save");
			Profiler.BeginSample("Quick Save");
#if DEBUG
			var stopWatch = new System.Diagnostics.Stopwatch();
			stopWatch.Start();
			Profiler.BeginSample("Serialize");
#endif
			gameSave = new GameSave("Quick Save", "1");
			gameSave.SetData(GameRegistry.GameMap.Serialize(), GameRegistry.GameState);
#if DEBUG
			stopWatch.Stop();
			Profiler.EndSample();
			Debug.Log($"Strip Excess Tile Data: {stopWatch.ElapsedMilliseconds}ms");
			stopWatch.Reset();
			stopWatch.Start();
			Profiler.BeginSample("convert to JSON and Write");
#endif
			gameSave.Save();
#if DEBUG
			stopWatch.Stop();
			Profiler.EndSample();
			Debug.Log($"JSON and Write: {stopWatch.ElapsedMilliseconds}ms");
			Profiler.EndSample();
#endif
			NotificationsUI.Notify(NotifType.Info, "Game Saved");
		}

		void Load()
		{
			Debug.Log("Quick Load");
			gameSave = GameSave.Load("Quick Save");
			//_renderer.SetMap(gameSave.map.Deserialize(GameRegistry.TileDatabase, GameRegistry.UnitDatabase), gameSave.gameState);
			NotificationsUI.Notify(NotifType.Info, "Game Loaded");
		}
	}
}
