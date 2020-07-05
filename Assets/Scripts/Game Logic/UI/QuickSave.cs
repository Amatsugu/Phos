using Amatsugu.Phos.DataStore;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Amatsugu.Phos.UI
{
	public class QuickSave : MonoBehaviour
	{
		private GameSave gameSave;
		private MapRenderer _renderer;

		private bool canSave = false;

		public void Start()
		{
			_renderer = GameObject.FindObjectOfType<MapRenderer>();
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
			if (Input.GetKeyUp(KeyCode.F6))
				Save();
			if (Input.GetKeyUp(KeyCode.F7))
				Load();
		}

		void Save()
		{
			Debug.Log("Quick Save");
			gameSave = new GameSave("Quick Save");
			gameSave.SetData(GameRegistry.GameMap.Serialize(), GameRegistry.GameState);
			gameSave.Save();
		}

		void Load()
		{
			Debug.Log("Quick Load");
			gameSave = GameSave.Load("Quick Save");
			_renderer.SetMap(gameSave.map.Deserialize(GameRegistry.TileDatabase, GameRegistry.UnitDatabase), gameSave.gameState);
		}
	}
}
