using Amatsugu.Phos.Tiles;

using AnimationSystem.AnimationData;
using AnimationSystem.Animations;

using System;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Building Decorators/Thumper")]
public class MineThumperDecorator : TileDecorator
{
	public float3[] offsets = new float3[2];
	public AnimationCurve[] curves = new AnimationCurve[2];

	private void OnValidate()
	{
		if (offsets.Length != 2)
			Array.Resize(ref offsets, 2);
		if (curves.Length != 2)
			Array.Resize(ref curves, 2);
	}

	public override int GetDecorEntityCount(Tile tile) => 2;

	public override void Render(Tile tile, NativeSlice<Entity> decor)
	{
		for (int i = 0; i < decor.Length; i++)
		{
			decor[i] = meshEntity.Instantiate(tile.SurfacePoint + offsets[i]);
			Map.EM.AddSharedComponentData(decor[i], new Slider
			{
				duration = 2,
				animationCurve = curves[i]
				//basePos = tile.SurfacePoint + offsets[i],
				//maxPos = tile.SurfacePoint + offsets[i] + new float3(0, .5f, 0),
			});
			
			Map.EM.AddComponentData(decor[i], new AnimationPhase
			{
				Value = Time.time + ((1 - i) * .1f)
			});
		}
	}
}