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
using Unity.Physics.Authoring;
using BoxCollider = Unity.Physics.BoxCollider;

[CreateAssetMenu(menuName = "Map Asset/Tile/Tile Info")]
public class TileInfo : MeshEntityRotatable
{
	[Header("Tile Info")]
	public string description;
	public TileDecorator[] decorators;
	public bool isTraverseable = true;
	public Faction faction;

	public override IEnumerable<ComponentType> GetComponents()
	{
		nonUniformScale = false;
		return base.GetComponents().Concat(new ComponentType[]{
			typeof(HexPosition),
			typeof(PhysicsCollider),
			typeof(PhysicsDebugDisplayData), 
			typeof(FactionId)
		});
	}

	public virtual Entity Instantiate(HexCoords pos, float height)
	{
		var e = Instantiate(new Vector3(pos.worldX, height, pos.worldZ), pos.edgeLength);
		Map.EM.SetComponentData(e, new HexPosition { coords = pos });
		Map.EM.SetComponentData(e, new FactionId { Value = faction });

		Map.EM.SetComponentData(e, new PhysicsDebugDisplayData
		{
			DrawColliders = 1
		});
		/*var collider = CylinderCollider.Create(new CylinderGeometry()
		{
			Center = new float3(0, -25, 0),
			Height = 50,
			Radius = pos.edgeLength,
			Orientation = quaternion.Euler(90, 30, 0),
			SideCount = 6,
			BevelRadius = 0
		}, CollisionFilter.Default, Unity.Physics.Material.Default);
		*/
		var collider = BoxCollider.Create(new BoxGeometry
		{
			BevelRadius = 0,
			Center = new float3(0, -25, 0),
			Size = new float3(1, 50, 1),
			Orientation = quaternion.identity
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
