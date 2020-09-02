using Amatsugu.Phos.TileEntities;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Unit Database")]
public class UnitDatabase : ScriptableObject, ISerializationCallbackReceiver
{
	[HideInInspector]
	public Dictionary<int, UnitDefination> unitEntites;
	[HideInInspector]
	public Dictionary<MobileUnitEntity, int> entityIds;

	[SerializeField]
	private int[] _ids;
	[SerializeField]
	private UnitDefination[] _defs;
	private int nextId = 0;


	public UnitDefination this[UnitIdentifier id] => unitEntites[id.id];

	public void OnAfterDeserialize()
	{
		unitEntites = new Dictionary<int, UnitDefination>();
		entityIds = new Dictionary<MobileUnitEntity, int>();
		for (int i = 0; i < _ids.Length; i++)
		{
			if(_defs[i].info == null)
			{
				Debug.LogError($"Unit failed to deserialize: {_ids[i]}");
				continue;
			}
			unitEntites.Add(_ids[i], _defs[i]);
			entityIds.Add(_defs[i].info, _ids[i]);
			if(_ids[i] > nextId)
				nextId = _ids[i];
		}
		nextId++;
	}

	public void Reset()
	{
		if (Application.isPlaying)
			return;
		nextId = 0;
		unitEntites = new Dictionary<int, UnitDefination>();
		entityIds = new Dictionary<MobileUnitEntity, int>();
		_defs = null;
		_ids = null;
	}

	public void OnBeforeSerialize()
	{
		if (unitEntites == null)
			return;
		_ids = unitEntites.Keys.ToArray();
		_defs = unitEntites.Values.ToArray();
	}

	[Serializable]
	public struct UnitDefination
	{
		public int id;
		public MobileUnitEntity info;
	}

	public bool RegisterUnit(MobileUnitEntity unit, out UnitDefination unitDef)
	{
		if (entityIds.ContainsKey(unit))
		{
			unitDef = default;
			return false;
		}
		unitDef = new UnitDefination
		{
			id = nextId++,
			info = unit,
		};

		unitEntites.Add(unitDef.id, unitDef);
		entityIds.Add(unit, unitDef.id);
		return true;
	}
}
