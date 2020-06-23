
using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
	[CreateAssetMenu(menuName = "Map Asset/Tile Decorators/Ice")]
	public class IceDecorator : TileDecorator
	{
		public float height;

		public override int GetDecorEntityCount(Tile tile)
		{
			return 0;
		}

		public override void Render(Tile tile, NativeSlice<Entity> decor)
		{
		}

		public override void UpdateHeight(NativeSlice<Entity> decor, Tile tile)
		{
			//Map.EM.SetComponentData(parent, new NonUniformScale { Value = new Vector3(1, Map.ActiveMap.seaLevel + height, 1) });
		}
	}
}