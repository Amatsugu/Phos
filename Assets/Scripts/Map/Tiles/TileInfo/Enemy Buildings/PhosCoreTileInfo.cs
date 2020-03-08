﻿using System.Collections.Generic;
using System.Linq;

using Unity.Entities;

using UnityEngine;

namespace Tiles.EnemyBuildings
{
	[CreateAssetMenu(menuName = "Map Asset/Tile/Enemy Building/Phos Core")]
	public class PhosCoreTileInfo : BuildingTileInfo
	{
		public float fireRate = 1;
		public float spinRate = 1;
		public float projectileSpeed = 1;
		public float targetingDelay = .2f;
		public int targetingRange = 10;

		public MeshEntityRotatable ring;

		public override IEnumerable<ComponentType> GetComponents()
		{
			return base.GetComponents().Append(typeof(PhosCore));
		}

		public override Entity Instantiate(HexCoords pos, float scale)
		{
			var e = base.Instantiate(pos, scale);

			Map.EM.AddComponentData(e, new PhosCore
			{
				fireRate = 1 / fireRate,
				spinRate = spinRate,
				nextVolleyTime = Time.time,
				projectileSpeed = projectileSpeed,
				targetingRange = targetingRange,
				targetDelay = targetingDelay,
				ring = ring.Instantiate(Map.ActiveMap[pos].SurfacePoint, Vector3.one)
			});

			return e;
		}

		public override Tile CreateTile(HexCoords pos, float height)
		{
			return new EnemyBuildingTile(pos, height, this);
		}
	}
}