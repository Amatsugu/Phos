using Amatsugu.Phos.Tiles;

using AnimationSystem.Animations;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset / Building Decorators / Wind Turbine")]
public class TurbineHeadDecorator : TileDecorator
{
	public float3 bladeOffset = Vector3.zero;

	public override int GetDecorEntityCount(Tile tile) => 1;

	public override void Render(Tile tile, NativeSlice<Entity> decor)
	{
		decor[0] = meshEntity.Instantiate(tile.SurfacePoint);
		Map.EM.AddComponentData(decor[0], new RotateAxis { Value = Vector3.up });
		Map.EM.AddComponentData(decor[0], new RotateSpeed { Value = math.radians(-40) });
	}

	public override void UpdateHeight(NativeSlice<Entity> decor, Tile tile)
	{
		Map.EM.SetComponentData(decor[0], new Translation { Value = tile.SurfacePoint + bladeOffset });
	}
}