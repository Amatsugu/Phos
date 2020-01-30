using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

namespace Tiles.EnemyBuildings
{
	[CreateAssetMenu(menuName = "Map Asset/Tile/Enemy Building/Phos Core")]
	public class PhosCoreTileInfo : BuildingTileInfo
	{
		public override IEnumerable<ComponentType> GetComponents()
		{
			return base.GetComponents().Append(typeof(PhosCore));
		}

		public override Entity Instantiate(HexCoords pos, Vector3 scale)
		{
			var e = base.Instantiate(pos, scale);

			Map.EM.AddComponentData(e, new PhosCore
			{
				fireRate = .1f,
				spinRate = 1,
				nextVolleyTime = Time.time
			});

			return e;
		}
	}
}
