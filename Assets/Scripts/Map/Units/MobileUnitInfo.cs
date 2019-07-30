using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Units/Unit")]
public class MobileUnitInfo : MeshEntityRotatable
{

	public enum UnitType
	{
		Land,
		Air,
		Naval
	}

	public float moveSpeed = 1;
	public int size;
	public UnitType type;

	public override IEnumerable<ComponentType> GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[] {
			typeof(MoveSpeed),
			typeof(Heading),
			typeof(UnitId)
		});
	}


	public Entity Instantiate(Vector3 pos, Quaternion rotation, int id)
	{
		var e = Instantiate(pos, Vector3.one, rotation);
		Map.EM.SetComponentData(e, new MoveSpeed { Value = moveSpeed });
		Map.EM.SetComponentData(e, new Heading { Value = Vector3.forward });
		Map.EM.SetComponentData(e, new UnitId { Value = id });
		return e;
	}
}
