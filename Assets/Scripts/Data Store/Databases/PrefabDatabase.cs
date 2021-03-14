using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amatsugu.Phos
{
    public class PrefabDatabase 
    {
        private Dictionary<GameObject, int> _prefabIndices;

		public int this[GameObject prefab] => _prefabIndices[prefab];

        public PrefabDatabase()
		{
            _prefabIndices = new Dictionary<GameObject, int>();
		}

		public bool RegisterPrefab(GameObject prefab, int index)
		{
            if(_prefabIndices.ContainsKey(prefab))
                return false;
            _prefabIndices.Add(prefab, index);
            return true;
		}
    }
}
