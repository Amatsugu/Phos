using Amatsugu.Phos.ECS;
using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.UnitComponents;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity.Entities;
using Unity.Rendering;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
	public class TurretTile : PoweredBuildingTile
	{
		public TurretTileEntity turretInfo;

		private Entity _turretHead;
		private Entity _turretBarrel;

		public TurretTile(HexCoords coords, float height, Map map, TurretTileEntity tInfo) : base(coords, height, map, tInfo)
		{
			turretInfo = tInfo;
		}

		public override void RenderBuilding()
		{
			base.RenderBuilding();
			var e = GetBuildingEntity();
			_turretHead = turretInfo.turretHead.Instantiate(SurfacePoint + turretInfo.headOffset, 1, GetBuildingRotation());
			if(turretInfo.turretBarrel != null)
				_turretBarrel = turretInfo.turretBarrel.Instantiate(SurfacePoint + turretInfo.barrelOffset, 1, GetBuildingRotation());
			Map.EM.AddComponentData(e, new Turret
			{
				Head = _turretHead,
				Barrel = _turretBarrel,
				shotOffset = turretInfo.shotOffset,
				projectile = turretInfo.projectileMesh.GetEntity()
			});
			
		}


		protected override void ApplyTileProperites()
		{
			base.ApplyTileProperites();
			var e = GetBuildingEntity();
			Map.EM.AddComponentData(e, new AttackRange
			{
				MaxRange = turretInfo.attackRange,
				MinRange = turretInfo.minAttackRange
			});
			Map.EM.AddComponentData(e, new AttackSpeed
			{
				Value = 1f/turretInfo.fireRate
			});
			switch(turretInfo.unitClass)
			{
				case UnitClass.Class.Turret:
					Map.EM.AddComponentData(e, new UnitClass.Turret());
					break;
				case UnitClass.Class.Artillery:
					Map.EM.AddComponentData(e, new UnitClass.Artillery());
					break;
				case UnitClass.Class.Support:
					Map.EM.AddComponentData(e, new UnitClass.Support());
					break;
				case UnitClass.Class.FixedGun:
					Map.EM.AddComponentData(e, new UnitClass.FixedGun());
					break;
			}
			switch (turretInfo.domain)
			{
				case UnitDomain.Domain.Air:
					Map.EM.AddComponentData(e, new UnitDomain.Air());
					break;
				case UnitDomain.Domain.Land:
					Map.EM.AddComponentData(e, new UnitDomain.Land());
					break;
				case UnitDomain.Domain.Naval:
					Map.EM.AddComponentData(e, new UnitDomain.Naval());
					break;
			}
			Map.EM.AddComponentData(e, new TargetingDomain
			{
				Value = turretInfo.targetingDomain
			});
		}

		public override void OnHide()
		{
			base.OnHide();
			Map.EM.AddComponent<FrozenRenderSceneTag>(_turretHead);
			if (turretInfo.turretBarrel != null)
				Map.EM.AddComponent<FrozenRenderSceneTag>(_turretBarrel);
		}

		public override void OnShow()
		{
			base.OnShow();
			Map.EM.RemoveComponent<FrozenRenderSceneTag>(_turretHead);
			if(turretInfo.turretBarrel != null)
				Map.EM.RemoveComponent<FrozenRenderSceneTag>(_turretBarrel);
		}

		public override void Destroy()
		{
			base.Destroy();
			Map.EM.DestroyEntity(_turretHead);
			if(turretInfo.turretBarrel != null)
				Map.EM.DestroyEntity(_turretBarrel);
		}
	}
}
