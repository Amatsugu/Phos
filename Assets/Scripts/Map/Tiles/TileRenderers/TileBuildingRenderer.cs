using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile Renderer/Building")]
public class TileBuildingRenderer : TileRenderer
{
	public int size = 3;
	public int influenceRange = 6;

	public override void Render(Tile tile, Entity parent)
	{
		//Map.ActiveMap.HexFlatten(tile.Coords, size, influenceRange);
		//var e = Map.EM.Instantiate(entity.GetEntity(true));
		//Map.EM.AddComponent(e, typeof(Parent));
		//Map.EM.SetComponentData(e, new Parent { Value = parent });
		Map.EM.SetComponentData(parent, new NonUniformScale { Value = Vector3.one });
		Map.EM.SetComponentData(parent, new Translation { Value = tile.SurfacePoint });
	}

	public override void UpdateHeight(Tile tile, Entity parent)
	{
		Map.EM.SetComponentData(parent, new NonUniformScale { Value = Vector3.one });
		Map.EM.SetComponentData(parent, new Translation { Value = tile.SurfacePoint });
	}
}
