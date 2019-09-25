using AnimationSystem.Animations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Building Decorators/Thumper")]
public class MineThumperDecorator : TileDecorator
{
	public Vector3[] offsets = new Vector3[2];
	public AnimationCurve[] curves = new AnimationCurve[2];

	void OnValidate()
	{
		if (offsets.Length != 2)
			Array.Resize(ref offsets, 2);
		if (curves.Length != 2)
			Array.Resize(ref curves, 2);
	}

	public override int GetDecorEntityCount(Tile tile) => 2;

	public override Entity[] Render(Tile tile)
	{
		var e = new Entity[2];
		for (int i = 0; i < e.Length; i++)
		{
			e[i] = meshEntity.Instantiate(tile.SurfacePoint + offsets[i]);
			Map.EM.AddSharedComponentData(e[i], new Slider
			{
				duration = 2,
				animationCurve = curves[i],
				basePos = tile.SurfacePoint + offsets[i],
				maxPos = tile.SurfacePoint + offsets[i] + new Vector3(0,.5f,0),
				phase = Time.time + ((1-i) * .1f)
			});
		}
		return e;
	}
}