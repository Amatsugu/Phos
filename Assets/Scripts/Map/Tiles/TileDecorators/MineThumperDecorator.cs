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

	void OnValidate()
	{
		if (offsets.Length != 2)
			Array.Resize(ref offsets, 2);
	}

	public override int GetDecorEntityCount(Tile tile) => 2;

	public override Entity[] Render(Tile tile)
	{
		var e = new Entity[2];
		for (int i = 0; i < e.Length; i++)
		{
			e[i] = meshEntity.Instantiate(tile.SurfacePoint + offsets[i]);
			Map.EM.AddComponentData(e[i], new Thumper { basePos = tile.Height + offsets[i].y, duration = 2, phase = Time.time - (i * .3f)});
		}
		return e;
	}
}