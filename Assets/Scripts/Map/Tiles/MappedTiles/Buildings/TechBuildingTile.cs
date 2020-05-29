using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechBuildingTile : PoweredBuildingTile
{
	private readonly TechBuildingEntity techInfo;

	private HashSet<HexCoords> _buffedTiles;

	public TechBuildingTile(HexCoords coords, float height, Map map, TechBuildingEntity tInfo) : base(coords, height, map, tInfo)
	{
		techInfo = tInfo;
		_buffedTiles = new HashSet<HexCoords>(HexCoords.SpiralSelect(coords, tInfo.effectRange, true));
	}

	public override void OnConnected()
	{
		base.OnConnected();
		UnlockBuildings();
	}

	protected override void OnBuilt()
	{
		base.OnBuilt();
		UnlockBuildings();	
	}

	protected override void OnBuiltAndPowered()
	{
		base.OnBuiltAndPowered();
		map.HexSelectForEach(Coords, techInfo.effectRange, t =>
		{
			if (t is BuildingTile b)
				ApplyBuff(b);
		}, true);
		map.OnTilePlaced += OnBuffedTileChanged;

	}

	private void OnBuffedTileChanged(HexCoords coords)
	{
		if (!_buffedTiles.Contains(coords))
			return;
		if (map[coords] is BuildingTile b)
			ApplyBuff(b);
	}

	protected virtual void ApplyBuff(BuildingTile building)
	{
		//TODO: Apply buffs
		Debug.Log($"A buff would be applied to {info.name}>{building.info.name}");
	}

	private void UnlockBuildings()
	{
		if (!IsBuilt || !HasHQConnection)
			return;
		for (int i = 0; i < techInfo.buildingsToUnlock.Length; i++)
			GameRegistry.UnlockBuilding(techInfo.buildingsToUnlock[i]);
	}

}
