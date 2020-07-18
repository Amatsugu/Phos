using Amatsugu.Phos.TileEntities;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Projectile Database")]
public class ProjectileDatabase : ScriptableObject, ISerializationCallbackReceiver
{
	[HideInInspector]
	public Dictionary<int, ProjectileDefination> entityDefs;
	[HideInInspector]
	public Dictionary<ProjectileMeshEntity, int> entityIds;

	[SerializeField]
	private int[] _ids;
	[SerializeField]
	private ProjectileDefination[] _defs;
	private int nextId = 0;

	public void OnAfterDeserialize()
	{
		entityDefs = new Dictionary<int, ProjectileDefination>();
		entityIds = new Dictionary<ProjectileMeshEntity, int>();
		for (int i = 0; i < _ids.Length; i++)
		{
			entityDefs.Add(_ids[i], _defs[i]);
			entityIds.Add(_defs[i].projectile, _ids[i]);
			if (_ids[i] > nextId)
				nextId = _ids[i];
		}
		nextId++;
	}

	public void Reset()
	{
		if (Application.isPlaying)
			return;
		nextId = 0;
		entityDefs = new Dictionary<int, ProjectileDefination>();
		entityIds = new Dictionary<ProjectileMeshEntity, int>();
		_defs = null;
		_ids = null;
	}

	public void OnBeforeSerialize()
	{
		if (entityDefs == null)
			return;
		_ids = entityDefs.Keys.ToArray();
		_defs = entityDefs.Values.ToArray();
	}

	[Serializable]
	public struct ProjectileDefination
	{
		public int id;
		public ProjectileMeshEntity projectile;
	}

	public bool RegisterUnit(ProjectileMeshEntity projectile, out ProjectileDefination projectileDef)
	{
		if (entityIds.ContainsKey(projectile))
		{
			projectileDef = entityDefs[entityIds[projectile]];
			return false;
		}
		projectileDef = new ProjectileDefination
		{
			id = nextId++,
			projectile = projectile,
		};

		entityDefs.Add(projectileDef.id, projectileDef);
		entityIds.Add(projectile, projectileDef.id);
		return true;
	}
}
