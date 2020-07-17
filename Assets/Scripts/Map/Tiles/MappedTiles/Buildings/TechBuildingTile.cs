using Amatsugu.Phos.TileEntities;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
	public class TechBuildingTile : PoweredBuildingTile
	{
		private readonly TechBuildingEntity techInfo;

		private readonly HashSet<HexCoords> _buffedTiles;

		public TechBuildingTile(HexCoords coords, float height, Map map, TechBuildingEntity tInfo) : base(coords, height, map, tInfo)
		{
			techInfo = tInfo;
			_buffedTiles = new HashSet<HexCoords>(HexCoords.SpiralSelect(coords, tInfo.effectRange, true));
		}

		protected override void OnBuiltAndPowered()
		{
			base.OnBuiltAndPowered();
			UnlockBuildings();
			map.HexSelectForEach(Coords, techInfo.effectRange, t =>
			{
				if (t is BuildingTile b)
					ApplyAOEBuff(b);
			}, true);
			map.OnBuildingBuilt += OnBuffedTileChanged;
		}

		private void OnBuffedTileChanged(HexCoords coords)
		{
			if (!_buffedTiles.Contains(coords))
				return;
			if (map[coords] is BuildingTile b)
				ApplyAOEBuff(b);
		}

		protected virtual void ApplyAOEBuff(BuildingTile building)
		{
			building.AddBuff(techInfo.StatsBuffs);
		}

		protected virtual void RemoveAOEBuff(BuildingTile building)
		{
			building.RemoveBuff(techInfo.StatsBuffs);
		}

		public override void OnRemoved()
		{
			for (int i = 0; i < techInfo.buildingsToUnlock.Length; i++)
			{
				GameRegistry.GameState.unlockedBuildings.Remove(techInfo.buildingsToUnlock[i].id);
				var b = GameRegistry.BuildingDatabase[techInfo.buildingsToUnlock[i]];
				NotificationsUI.Notify(b.info.icon, $"Building Locked: {b.info.GetNameString()}", $"{info.GetNameString()} was destroyed, rebuild it in order to regain acesss.");
			}
			base.OnRemoved();
			map.HexSelectForEach(Coords, techInfo.effectRange, t =>
			{
				if (t is BuildingTile b)
					RemoveAOEBuff(b);
			}, true);
		}

		public override void OnDisconnected()
		{
			base.OnDisconnected();
			map.HexSelectForEach(Coords, techInfo.effectRange, t =>
			{
				if (t is BuildingTile b)
					RemoveAOEBuff(b);
			}, true);
		}

		private void UnlockBuildings()
		{
			if (!IsBuilt || !HasHQConnection)
				return;
			for (int i = 0; i < techInfo.buildingsToUnlock.Length; i++)
				GameRegistry.UnlockBuilding(techInfo.buildingsToUnlock[i]);
		}

		public override void OnPlaced()
		{
			base.OnPlaced();
		}
	}
}