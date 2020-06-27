﻿using Amatsugu.Phos.ECS;
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
	public class TurretTile : BuildingTile
	{
		public TurretTileEntity turretTile;

		private Entity _turretHead;

		public TurretTile(HexCoords coords, float height, Map map, TurretTileEntity tInfo) : base(coords, height, map, tInfo)
		{
			turretTile = tInfo;
		}

		public override void RenderBuilding()
		{
			base.RenderBuilding();
			var e = GetBuildingEntity();
			_turretHead = turretTile.turretHead.Instantiate(SurfacePoint);
			Map.EM.AddComponentData(e, new Turret
			{
				Head = _turretHead
			});
		}


		protected override void PrepareEntity()
		{
			base.PrepareEntity();
			var e = GetBuildingEntity();
			Map.EM.AddComponentData(e, new AttackRange
			{
				Value = turretTile.attackRange,
				ValueSq = turretTile.attackRange * turretTile.attackRange
			});
			Map.EM.AddComponentData(e, new AttackSpeed
			{
				Value = turretTile.fireRate
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
