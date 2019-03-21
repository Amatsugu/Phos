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

	public override void Render(Tile tile, Entity tileEntity)
	{
		//Map.EM.AddComponent(e, typeof(Parent));
		//Map.EM.SetComponentData(e, new Parent { Value = parent });
		//Map.EM.AddComponent(tileEntity, typeof(ProductionData));
		Map.EM.AddSharedComponentData(tileEntity, new ProductionData {
			resourceIds = new int[] //TODO: Implement resource maneger to handle assignment of resource ids
			{
				0,
				1,
				2
			},
			productionRates = new int[] { 100, 100, 100 }
		});
		//Map.EM.SetComponentData(tileEntity, new ResourceStorageData
		//{
			//resourceIds = new int[] { 0, 1, 2}, //TODO: Iterate all resources
			//count = new int[] { 100, 100, 100},
			//max = new int[] {100, 100, 100}
		//});
		Map.EM.SetComponentData(tileEntity, new NonUniformScale { Value = Vector3.one });
		Map.EM.SetComponentData(tileEntity, new Translation { Value = tile.SurfacePoint });
	}

	public override void UpdateHeight(Tile tile, Entity parent)
	{
		Map.EM.SetComponentData(parent, new NonUniformScale { Value = Vector3.one });
		Map.EM.SetComponentData(parent, new Translation { Value = tile.SurfacePoint });
	}
}
