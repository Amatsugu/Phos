using System.Collections.Generic;
using System.Linq;

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Tiles.EnemyBuildings
{
	[CreateAssetMenu(menuName = "Map Asset/Tile/Enemy Building/Phos Core")]
	public class PhosCoreTileInfo : BuildingTileEntity
	{
		[Header("Stats")]
		public float fireRate = 1;
		public float spinRate = 1;
		public float projectileSpeed = 1;
		public float targetingDelay = .2f;
		public int targetingRange = 10;
		[Header("Child Entities")]
		public MeshEntityRotatable ring;
		
		[Header("Projectiles")]
		public MeshEntityRotatable projectile;
		public MeshEntityRotatable laser;

		public override IEnumerable<ComponentType> GetComponents()
		{
			return base.GetComponents().Concat(new ComponentType[] 
			{
				//typeof(PhosCore),
				//typeof(PhosCoreData),
			});
		}

		public override void PrepareDefaultComponentData(Entity entity)
		{
			base.PrepareDefaultComponentData(entity);
			
		}

		public override Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return new PhosCoreTile(pos, height, map, this);
		}
	}
}