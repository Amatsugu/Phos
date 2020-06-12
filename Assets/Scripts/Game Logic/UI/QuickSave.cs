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
		private SerializedMap _map;
		private MapRenderer _renderer;

		public void Start()
		{
			_renderer = GameObject.FindObjectOfType<MapRenderer>();
		}

		public void Update()
		{
			if (Input.GetKeyUp(KeyCode.F6))
				Save();
			if (Input.GetKeyUp(KeyCode.F7))
				Load();
		}

		void Save()
		{
			Debug.Log("Quick Save");
			_map = GameRegistry.GameMap.Serialize();
			File.WriteAllText("quicksave.json", JsonConvert.SerializeObject(_map));
		}

		void Load()
		{
			Debug.Log("Quick Load");
			_renderer.SetMap(_map.Deserialize(GameRegistry.TileDatabase, GameRegistry.UnitDatabase));
		}
	}
}
