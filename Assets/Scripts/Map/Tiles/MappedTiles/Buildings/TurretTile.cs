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

		public TurretTile(HexCoords coords, float height, Map map, TurretTileEntity tInfo) : base(coords, height, map, tInfo)
		{
			turretInfo = tInfo;
		}

		public override void RenderBuilding()
		{
			base.RenderBuilding();
			var e = GetBuildingEntity();
			_turretHead = turretInfo.turretHead.Instantiate(SurfacePoint + turretInfo.headOffset);
			Map.EM.AddComponentData(e, new Turret
			{
				Head = _turretHead,
				shotOffset = turretInfo.barrelOffset
			});
		}


		protected override void ApplyTileProperites()
		{
			base.ApplyTileProperites();
			var e = GetBuildingEntity();
			Map.EM.AddComponentData(e, new AttackRange
			{
				Value = turretInfo.attackRange,
				ValueSq = turretInfo.attackRange * turretInfo.attackRange
			});
			Map.EM.AddComponentData(e, new AttackSpeed
			{
				Value = 1f/turretInfo.fireRate
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
