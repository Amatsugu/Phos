using Amatsugu.Phos.ECS;
using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.UnitComponents;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
	public class TurretTile : PoweredBuildingTile
	{
		public TurretTileEntity turretInfo;

		private Entity _turretHead;
		private Entity _turretBarrel;
		private Entity _turretBarrelTip;

		public TurretTile(HexCoords coords, float height, Map map, TurretTileEntity tInfo, int rotation) : base(coords, height, map, tInfo, rotation)
		{
			turretInfo = tInfo;
		}

		public override void RenderSubMeshes(quaternion rot)
		{
			base.RenderSubMeshes(rot);
			var e = GetBuildingEntity();
			_turretHead = subMeshes[turretInfo.turretHead.id];
			if(turretInfo.turretBarrel.id != -1)
				_turretBarrel = subMeshes[turretInfo.turretBarrel.id];
			if (turretInfo.turretBarrelTip.id != -1)
				_turretBarrelTip = subMeshes[turretInfo.turretBarrelTip.id];
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
			//Map.EM.SetComponentData(_turretBarrel, new Parent {Value = _turretHead });
			//Map.EM.SetComponentData(_turretBarrelTip, new Parent {Value = _turretBarrel });
			switch (turretInfo.unitClass)
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
			Map.EM.AddComponent<DisableRendering>(_turretHead);
			if (Map.EM.Exists(_turretBarrel))
				Map.EM.AddComponent<DisableRendering>(_turretBarrel);
		}

		public override void OnShow()
		{
			base.OnShow();
			Map.EM.RemoveComponent<DisableRendering>(_turretHead);
			if (Map.EM.Exists(_turretBarrel))
				Map.EM.RemoveComponent<DisableRendering>(_turretBarrel);
		}

		public override void Destroy()
		{
			base.Destroy();
			if (World.DefaultGameObjectInjectionWorld == null)
				return;
			Map.EM.DestroyEntity(_turretHead);
			if (Map.EM.Exists(_turretBarrel))
				Map.EM.DestroyEntity(_turretBarrel);
		}
	}
}
