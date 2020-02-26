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
		nonUniformScale = false;
		return base.GetComponents().Concat(new ComponentType[]{
			typeof(HexPosition),
			typeof(PhysicsCollider)
		});
	}

	public virtual Entity Instantiate(HexCoords pos, float height)
	{
		var e = Instantiate(new Vector3(pos.worldX, height, pos.worldZ), pos.edgeLength);
		Map.EM.SetComponentData(e, new HexPosition { coords = pos });


		var collider = CylinderCollider.Create(new CylinderGeometry()
		{
			Center = new float3(0, -25, 0),
			Height = 50,
			Radius = pos.edgeLength,
			Orientation = quaternion.Euler(270, 270, 0),
			SideCount = 6,
			BevelRadius = 0
		}, CollisionFilter.Default, Unity.Physics.Material.Default);

		

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
