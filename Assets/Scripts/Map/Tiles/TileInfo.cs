using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/TileInfo")]
public class TileInfo : ScriptableObject
{
	public GameObject tilePrefab;

	private Entity _entity;

	public Entity GetEntity(EntityManager em)
	{
		if(_entity == null || !em.Exists(_entity))
			return _entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(tilePrefab, em.World);
		return _entity;
	}
}
