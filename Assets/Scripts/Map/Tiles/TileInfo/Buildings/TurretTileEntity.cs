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
		public float minAttackRange;
		public ProjectileMeshEntity projectileMesh;
		//[CreateNewAsset("Assets/GameData/MapAssets/Meshes/Buildings", typeof(MeshEntityRotatable))]
		public SubMeshIdentifier turretHead;
		//[CreateNewAsset("Assets/GameData/MapAssets/Meshes/Buildings", typeof(MeshEntityRotatable))]
		public SubMeshIdentifier turretBarrel;
		public SubMeshIdentifier turretBarrelTip;
		public UnitClass.Class unitClass;
		public UnitDomain.Domain domain;
		[EnumFlags]
		public UnitDomain.Domain targetingDomain;
		//public float3 headOffset;
		//public float3 barrelOffset;
		public float3 shotOffset;

		public override IEnumerable<ComponentType> GetComponents()
		{
			return base.GetComponents().Concat(new ComponentType[] 
			{ 
				typeof(Turret), 
				typeof(TargetingDomain),
				typeof(AttackSpeed),
				typeof(AttackRange)
			});
		}

		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return CreateTile(map, pos, height, 0);
		}

		public override Tile CreateTile(Map map, HexCoords pos, float height, int rotation)
		{
			return new TurretTile(pos, height, map, this, rotation);
		}
	}
}
