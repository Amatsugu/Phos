using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;
using static PhosCoreSystem;

namespace Tiles.EnemyBuildings
{
	[CreateAssetMenu(menuName = "Map Asset/Tile/Enemy Building/Phos Core")]
	public class PhosCoreTileInfo : BuildingTileInfo
	{
		public float fireRate = 1;
		public float spinRate = 1;
		public float projectileSpeed = 1;
		public float targetingDelay = .2f;
		public float targetingRange = 10;

		public override IEnumerable<ComponentType> GetComponents()
		{
			return base.GetComponents().Append(typeof(PhosCore));
		}

		public override Entity Instantiate(HexCoords pos, Vector3 scale)
		{
			var e = base.Instantiate(pos, scale);

			Map.EM.AddComponentData(e, new PhosCore
			{
				fireRate = 1/fireRate,
				spinRate = spinRate,
				nextVolleyTime = Time.time,
				projectileSpeed = projectileSpeed,
				targetingRange = targetingRange,
				targetDelayRatio = targetingDelay
			});

			return e;
		}

		private void OnValidate()
		{
			targetingDelay = Mathf.Clamp(targetingDelay, 0, 1);
		}
	}
}
