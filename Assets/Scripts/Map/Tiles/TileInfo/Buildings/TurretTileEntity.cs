using Amatsugu.Phos.Tiles;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity.Entities;

using UnityEngine;

namespace Amatsugu.Phos.TileEntities
{
	[CreateAssetMenu(menuName = "Map Asset/Tile/Building/Turret")]
	public class TurretTileEntity : BuildingTileEntity
	{
		[Header("Turret")]
		public float fireRate;
		public float damage;
		public float attackRange;
		public ProjectileMeshEntity projectileMesh;
		public MeshEntityRotatable turretHead;

		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return new TurretTile(pos, height, map, this);
		}
	}
}
