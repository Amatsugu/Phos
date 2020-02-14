using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

[CreateAssetMenu(menuName = "Map Asset/Tile/Tile Info")]
public class TileInfo : MeshEntityRotatable
{
	[Header("Tile Info")]
	public string description;
    public TileDecorator[] decorators;
	public bool isTraverseable = true;

	public override IEnumerable<ComponentType> GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[]{
			typeof(HexPosition),
			typeof(PhysicsCollider)
		});
	}

	public virtual Entity Instantiate(HexCoords pos, Vector3 scale)
	{
		var e = Instantiate(pos.worldXZ, scale);
		Map.EM.SetComponentData(e, new HexPosition { coords = pos });


		var collider = CylinderCollider.Create(new CylinderGeometry()
		{
			Center = pos.worldXZ + new Vector3(0, scale.y /2f, 0),
			Height = scale.y,
			Radius = pos.edgeLength,
			Orientation = quaternion.identity,
			SideCount = 6,
			BevelRadius = 0
		});

		

		Map.EM.SetComponentData(e, new PhysicsCollider
		{
			Value = collider
		});

		return e;
	}

	public virtual Tile CreateTile(HexCoords pos, float height)
	{
		return new Tile(pos, height, this);
	}
}
