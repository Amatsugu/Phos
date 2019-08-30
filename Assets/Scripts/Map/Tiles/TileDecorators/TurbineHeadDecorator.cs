using AnimationSystem.Animations;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset / Building Decorators / Wind Turbine")]
public class TurbineHeadDecorator : TileDecorator
{
	public MeshEntityRotatable turbineBlade;
	public Vector3 bladeOffset = new Vector3(0, 1.56f, -.286f);

	public override int GetDecorEntityCount(Tile tile) => 2;

	public override Entity[] Render(Tile tile)
	{
		var entities = new Entity[2];
		entities[0] = meshEntity.Instantiate(tile.SurfacePoint);
		entities[1] = turbineBlade.Instantiate(tile.SurfacePoint + bladeOffset);
		Map.EM.AddComponentData(entities[1], new RotateAxis { Value = Vector3.forward });
		Map.EM.AddComponentData(entities[1], new RotateSpeed { Value = -2 });
		return entities;
	}

	public override void UpdateHeight(NativeSlice<Entity> decor, Tile tile)
	{
		Map.EM.SetComponentData(decor[0], new Translation { Value = tile.SurfacePoint });
		Map.EM.SetComponentData(decor[1], new Translation { Value = tile.SurfacePoint + bladeOffset });
	}
}
