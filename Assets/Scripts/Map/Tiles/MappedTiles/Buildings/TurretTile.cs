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
			if(turretInfo.turretHead != null)
				_turretBarrel = turretInfo.turretBarrel.Instantiate(SurfacePoint + turretInfo.barrelOffset, 1, GetBuildingRotation());
			Map.EM.SetComponentData(e, new Turret
			{
				Head = _turretHead,
				Barrel = _turretBarrel,
				shotOffset = turretInfo.shotOffset
			});
			
		}


		protected override void ApplyTileProperites()
		{
			base.ApplyTileProperites();
			var e = GetBuildingEntity();
			Map.EM.SetComponentData(e, new AttackRange
			{
				Value = turretInfo.attackRange,
				ValueSq = turretInfo.attackRange * turretInfo.attackRange
			});
			Map.EM.SetComponentData(e, new AttackSpeed
			{
				Value = 1f/turretInfo.fireRate
			});
			IComponentData comp = default;
			switch(turretInfo.unitClass)
			{
				case UnitClass.Class.Artillery:
					comp = new UnitClass.Artillery();
					break;
				case UnitClass.Class.Support:
					comp = new UnitClass.Support();
					break;
				case UnitClass.Class.FixedGun:
					comp = new UnitClass.FixedGun();
					break;
				case UnitClass.Class.Turret:
					comp = new UnitClass.Turret();
					break;
			}
			Map.EM.SetComponentData(e, comp);
			switch (turretInfo.domain)
			{
				case UnitDomain.Domain.Air:
					comp = new UnitDomain.Air();
					break;
				case UnitDomain.Domain.Land:
					comp = new UnitDomain.Land();
					break;
				case UnitDomain.Domain.Naval:
					comp = new UnitDomain.Naval();
					break;
			}
			Map.EM.SetComponentData(e, comp);
			Map.EM.SetComponentData(e, new TagetingDomain
			{
				Value = turretInfo.targetingDomain
			});
		}

		public override void OnHide()
		{
			base.OnHide();
			Map.EM.AddComponent<FrozenRenderSceneTag>(_turretHead);
		}

		public override void OnShow()
		{
			base.OnShow();
			Map.EM.RemoveComponent<FrozenRenderSceneTag>(_turretHead);
		}

		public override void Destroy()
		{
			base.Destroy();
			Map.EM.DestroyEntity(_turretHead);
		}
	}
}
