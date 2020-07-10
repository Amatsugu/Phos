using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Profiling;

namespace Amatsugu.Phos.DataStore
{
	public class GameSave
	{
		[JsonIgnore]
		public readonly string name;
		[JsonIgnore]
		public SerializedMap map;
		public GameState gameState;
		public string version;

		public GameSave(string name, string version)
		{
			this.name = name;
			this.version = version;
		}

		public void SetData(SerializedMap map, GameState gameState)
		{
			this.map = map;
			this.gameState = gameState;
			Array.Copy(ResourceSystem.resCount, gameState.resCount, ResourceSystem.resCount.Length);
		}

		public void Save()
		{
			var dir = $"{Application.dataPath}/Saves/{name}";
			if(!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			var formating = Formatting.None;
#if UNITY_EDITOR
			formating = Formatting.Indented;
#endif
			//File.WriteAllText($"{dir}/gamestate.json", JsonConvert.SerializeObject(gameState, formating));
			Profiler.BeginSample("Convert to JSON");
			var mapJson = JsonConvert.SerializeObject(map, formating);
			var saveJson = JsonConvert.SerializeObject(this, formating);
			Profiler.EndSample();
			Profiler.BeginSample("Save to Disk");
			File.WriteAllText($"{dir}/map.json", mapJson);
			File.WriteAllText($"{dir}/save.json", saveJson);
			Profiler.EndSample();
		}

		public static GameSave Load(string saveName)
		{
			var dir = $"{Application.dataPath}/Saves/{saveName}";
			var save = JsonConvert.DeserializeObject<GameSave>(File.ReadAllText($"{dir}/save.json"));
			var map = JsonConvert.DeserializeObject<SerializedMap>(File.ReadAllText($"{dir}/map.json"));
			//var state = JsonConvert.DeserializeObject<GameState>(File.ReadAllText($"{dir}/gamestate.json"));
			save.map = map;
			return save;
		}
	}
}
