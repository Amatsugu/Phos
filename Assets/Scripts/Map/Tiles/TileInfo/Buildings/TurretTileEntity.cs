using Amatsugu.Phos.ECS;
using Amatsugu.Phos.Tiles;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity.Entities;
using Unity.Mathematics;

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
		[CreateNewAsset("Assets/GameData/MapAssets/Meshes/Buildings", typeof(MeshEntityRotatable))]
		public MeshEntityRotatable turretHead;
		[CreateNewAsset("Assets/GameData/MapAssets/Meshes/Buildings", typeof(MeshEntityRotatable))]
		public MeshEntityRotatable turretBarrel;
		public UnitClass.Class unitClass;
		[EnumFlags]
		public UnitDomain.Domain domain;
		public float3 headOffset;
		public float3 barrelOffset;
		public float3 shotOffset;

		public override IEnumerable<ComponentType> GetComponents()
		{
			return base.GetComponents().Concat(new ComponentType[] { typeof(Turret), typeof(UnitDomain), typeof(UnitClass), typeof(AttackSpeed), typeof(AttackRange) });
		}

		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return new TurretTile(pos, height, map, this);
		}
	}
}
