using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amatsugu.Phos
{
    public class PrefabDatabase 
    {
        private Dictionary<int, int> _prefabIndices;

		public int this[GameObject prefab] => _prefabIndices[prefab.GetInstanceID()];

        public PrefabDatabase()
		{
            _prefabIndices = new Dictionary<int, int>();
		}

		public bool RegisterPrefab(GameObject prefab, int index)
		{
            if(_prefabIndices.ContainsKey(prefab.GetInstanceID()))
                return false;
            _prefabIndices.Add(prefab.GetInstanceID(), index);
            return true;
		}
    }
}
