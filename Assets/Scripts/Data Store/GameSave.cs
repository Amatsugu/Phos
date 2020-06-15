using System;
using System.Collections.Generic;
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
			var dir = Application.persistentDataPath;
			Debug.Log(dir);
		}

		public static GameSave Load(string baseDir)
		{
			GameSave map = default;
			return map;
		}
	}
}
