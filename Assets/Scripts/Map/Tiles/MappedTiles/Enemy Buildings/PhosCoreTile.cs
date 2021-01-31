
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
		private Entity _ringEntity;
		private readonly PhosCoreTileInfo _phosInfo;

		public PhosCoreTile(HexCoords coords, float height, Map map, PhosCoreTileInfo tInfo, int rotation) : base(coords, height, map, tInfo, rotation)
		{
			isBuilt = true;
			_phosInfo = tInfo;
		}

		protected override void ApplyTileProperites()
		{
			base.ApplyTileProperites();
			var e = GetBuildingEntity();
			Map.EM.AddComponentData(e, new PhosCoreData
			{
				ring = _ringEntity = _phosInfo.ring.Instantiate(SurfacePoint, 1),
				laser = _phosInfo.laser.GetEntity(),
				projectile = _phosInfo.projectile.GetEntity()
			});
			Map.EM.AddComponentData(e, new PhosCore
			{
				fireRate = 1f / _phosInfo.fireRate,
				spinRate = _phosInfo.spinRate,
				nextVolleyTime = Time.time,
				projectileSpeed = _phosInfo.projectileSpeed,
				targetingRangeSq = _phosInfo.targetingRange * _phosInfo.targetingRange,
				targetingRange = _phosInfo.targetingRange,
				targetDelay = _phosInfo.targetingDelay,
			});
		}

		public override void OnShow()
		{
			base.OnShow();
			Map.EM.RemoveComponent<DisableRendering>(_ringEntity);
		}

		public override void OnHide()
		{
			base.OnHide();
			Map.EM.AddComponent<DisableRendering>(_ringEntity);
		}

		public override void OnDeath()
		{
			//base.OnDeath();
		}

		public override void Destroy()
		{
			base.Destroy();
			if(World.DefaultGameObjectInjectionWorld != null)
				Map.EM.DestroyEntity(_ringEntity);
		}

		protected override void OnBuilt()
		{
			//base.OnBuilt();
		}
	}
}