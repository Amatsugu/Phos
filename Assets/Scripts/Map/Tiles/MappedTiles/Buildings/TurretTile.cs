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

		public TurretTile(HexCoords coords, float height, Map map, TurretTileEntity tInfo, int rotation) : base(coords, height, map, tInfo, rotation)
		{
			turretInfo = tInfo;
		}


		public override void PrepareBuildingEntity(Entity building, EntityCommandBuffer postUpdateCommands)
		{
			base.PrepareBuildingEntity(building, postUpdateCommands);
			postUpdateCommands.AddComponent(building, new AttackRange
			{
				MaxRange = turretInfo.attackRange,
				MinRange = turretInfo.minAttackRange
			});
			postUpdateCommands.AddComponent(building, new AttackSpeed
			{
				Value = 1f / turretInfo.fireRate
			});
			//Map.EM.SetComponentData(_turretBarrel, new Parent {Value = _turretHead });
			//Map.EM.SetComponentData(_turretBarrelTip, new Parent {Value = _turretBarrel });
			switch (turretInfo.unitClass)
			{
				case UnitClass.Class.Turret:
					postUpdateCommands.AddComponent(building, new UnitClass.Turret());
					break;
				case UnitClass.Class.Artillery:
					postUpdateCommands.AddComponent(building, new UnitClass.Artillery());
					break;
				case UnitClass.Class.Support:
					postUpdateCommands.AddComponent(building, new UnitClass.Support());
					break;
				case UnitClass.Class.FixedGun:
					postUpdateCommands.AddComponent(building, new UnitClass.FixedGun());
					break;
			}
			switch (turretInfo.domain)
			{
				case UnitDomain.Domain.Air:
					postUpdateCommands.AddComponent(building, new UnitDomain.Air());
					break;
				case UnitDomain.Domain.Land:
					postUpdateCommands.AddComponent(building, new UnitDomain.Land());
					break;
				case UnitDomain.Domain.Naval:
					postUpdateCommands.AddComponent(building, new UnitDomain.Naval());
					break;
			}
			postUpdateCommands.AddComponent(building, new TargetingDomain
			{
				Value = turretInfo.targetingDomain
			});
		}
	}
}
