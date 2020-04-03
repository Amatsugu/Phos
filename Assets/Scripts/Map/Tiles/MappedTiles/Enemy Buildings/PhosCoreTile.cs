using Tiles.EnemyBuildings;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

using UnityEngine;

public class PhosCoreTile : EnemyBuildingTile
{
	private Entity _ringEntity;
	private readonly PhosCoreTileInfo _phosInfo;

	public PhosCoreTile(HexCoords coords, float height, PhosCoreTileInfo tInfo) : base(coords, height, tInfo)
	{
		_isBuilt = true;
		_phosInfo = tInfo;
	}

	protected override void PrepareEntity()
	{
		base.PrepareEntity();
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
		Map.EM.RemoveComponent<FrozenRenderSceneTag>(_ringEntity);
	}

	public override void OnHide()
	{
		base.OnHide();
		Map.EM.AddComponent<FrozenRenderSceneTag>(_ringEntity);
	}

	public override void OnDeath()
	{
		//base.OnDeath();
	}

	public override void Destroy()
	{
		base.Destroy();
		Map.EM.DestroyEntity(_ringEntity);
	}

	protected override void OnBuilt()
	{
		//base.OnBuilt();
	}
}