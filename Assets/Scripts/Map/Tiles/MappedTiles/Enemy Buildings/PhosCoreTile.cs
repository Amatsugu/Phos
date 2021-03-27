
using Amatsugu.Phos.TileEntities;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
	public class PhosCoreTile : EnemyBuildingTile
	{
		private readonly PhosCoreTileInfo _phosInfo;

		public PhosCoreTile(HexCoords coords, float height, Map map, PhosCoreTileInfo tInfo, int rotation) : base(coords, height, map, tInfo, rotation)
		{
			isBuilt = true;
			_phosInfo = tInfo;
		}

		public override void PrareBuildingEntity(Entity building, EntityCommandBuffer postUpdateCommands)
		{
			base.PrareBuildingEntity(building, postUpdateCommands);
			//TODO: Move to conversion system
			//postUpdateCommands.AddComponent(building, new PhosCoreData
			//{
			//	ring = _ringEntity = _phosInfo.ring.Instantiate(SurfacePoint, 1),
			//	laser = _phosInfo.laser.GetEntity(),
			//	projectile = _phosInfo.projectile.GetEntity()
			//});
			//postUpdateCommands.AddComponent(building, new PhosCore
			//{
			//	fireRate = 1f / _phosInfo.fireRate,
			//	spinRate = _phosInfo.spinRate,
			//	nextVolleyTime = Time.time,
			//	projectileSpeed = _phosInfo.projectileSpeed,
			//	targetingRangeSq = _phosInfo.targetingRange * _phosInfo.targetingRange,
			//	targetingRange = _phosInfo.targetingRange,
			//	targetDelay = _phosInfo.targetingDelay,
			//});
		}

		public override void OnDeath()
		{
			//base.OnDeath();
		}

		public override void Dispose()
		{
			base.Dispose();
		}

		protected override void OnBuilt()
		{
			//base.OnBuilt();
		}
	}
}