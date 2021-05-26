using Amatsugu.Phos;
using Amatsugu.Phos.Tiles;

using System;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

using UnityEngine;

[Serializable]
public abstract class TileDecorator : ScriptableObject
{
	public GameObject basePrefab;

	public float densityMulti = 1;

	public abstract int GetDecorEntityCount(Tile tile);

	public virtual void Instantiate(Entity tileInst, HexCoords coords, ref DynamicBuffer<GenericPrefab> genericPrefabs, EntityCommandBuffer postUpdateCommands)
	{
		if (basePrefab == null)
			return;
		var prefab = genericPrefabs[GameRegistry.PrefabDatabase[basePrefab]];
		var instance = postUpdateCommands.Instantiate(prefab.value);
		postUpdateCommands.AddComponent(instance, new Parent { Value = tileInst });
		postUpdateCommands.AddComponent<LocalToParent>(instance);
	}

	public virtual void DeclarePrefabs(List<GameObject> prefabs)
	{
		if (basePrefab == null)
			return;
		prefabs.Add(basePrefab);
	}
}