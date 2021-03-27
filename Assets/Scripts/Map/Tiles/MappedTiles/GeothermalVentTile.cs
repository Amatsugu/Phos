using Amatsugu.Phos.TileEntities;

using Unity.Entities;
using Unity.Rendering;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
	public class GeothermalVentTile : ResourceTile
	{
		public VentTileInfo ventInfo;

		private Entity _core;
		private GameObject _gyser;

		public GeothermalVentTile(HexCoords coords, float height, Map map, VentTileInfo tInfo = null) : base(coords, height, map, tInfo)
		{
			ventInfo = tInfo;
		}

		public override TileEntity MeshEntity => originalTile;

		public override Entity InstantiateTile(DynamicBuffer<GenericPrefab> prefabs, EntityCommandBuffer postUpdateCommands)
		{
			_gyser = GameObject.Instantiate(ventInfo.gyser, new Vector3(Coords.WorldPos.x, Height, Coords.WorldPos.z), Quaternion.identity);
			return base.InstantiateTile(prefabs, postUpdateCommands);
		}
		
		public override void Dispose()
		{
			base.Dispose();
			Object.Destroy(_gyser);
		}
	}

	//TODO: 
	public class GeothermalVentShellTile : ResourceTile
	{
		public VentTileInfo ventInfo;
		public float angle;
		public Entity _shell;

		public GeothermalVentShellTile(HexCoords coords, float height, float angle, Map map, VentTileInfo tInfo = null) : base(coords, height, map, tInfo)
		{
			ventInfo = tInfo;
			this.angle = angle;
		}

		public override TileEntity MeshEntity => originalTile;

		public override void Dispose()
		{
			base.Dispose();
		}
	}
}