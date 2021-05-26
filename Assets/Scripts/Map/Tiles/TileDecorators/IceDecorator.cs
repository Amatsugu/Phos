
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
	}
}