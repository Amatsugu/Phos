using Amatsugu.Phos.ECS;
using Amatsugu.Phos.Tiles;

using Unity.Mathematics;

using UnityEngine;

namespace Amatsugu.Phos.TileEntities
{
	public class WindTurbineTileEntity : BuildingTileEntity
	{
		[Header("Wind Turbine")]
		public float maxSpinSpeed = 10;
		public float2 efficencyRange;
		public SubMeshIdentifier turbineBladeSubMesh;

		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return new WindTurbileTile(pos, height, map, this);
		}
	}
}