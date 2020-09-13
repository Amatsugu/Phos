using System.Collections;
using System.Collections.Generic;

using Unity.Entities;
using Unity.Rendering;

using UnityEngine;

public class HDRPMaterialSystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		return;
		Entities.WithNone<Disabled, FrozenRenderSceneTag>().ForEach((Entity e, RenderMesh mesh, ref HDRPMateiralColor color) =>
		{
			mesh.material.SetColor("_BaseColor", Random.ColorHSV());
		});
	}
}
