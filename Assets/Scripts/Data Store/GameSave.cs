using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Amatsugu.Phos.DataStore
{
	public class GameSave
	{
		public readonly string name;
		public SerializedMap map;
		public GameState gameState;

		public GameSave(string name)
		{
			this.name = name;
		}

		public void SetData(SerializedMap map, GameState gameState)
		{
			this.map = map;
			this.gameState = gameState;
			Array.Copy(ResourceSystem.resCount, gameState.resCount, ResourceSystem.resCount.Length);
		}

		public void Save()
		{
			var dir = $"{Application.persistentDataPath}/Saves/{name}";
			Debug.Log(dir);
			if(!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			File.WriteAllText($"{dir}/gamestate.json", JsonConvert.SerializeObject(gameState));
			File.WriteAllText($"{dir}/map.json", JsonConvert.SerializeObject(map));
		}

		public static GameSave Load(string saveName)
		{
			var dir = $"{Application.persistentDataPath}/Saves/{saveName}";
			var map = JsonConvert.DeserializeObject<SerializedMap>(File.ReadAllText($"{dir}/map.json"));
			var state = JsonConvert.DeserializeObject<GameState>(File.ReadAllText($"{dir}/gamestate.json"));
			GameSave save = new GameSave(saveName)
			{
				gameState = state,
				map = map
			};
			return save;
		}
	}
}
