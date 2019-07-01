using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ResourceConduitTile : PoweredBuildingTile
{
	public ResourceConduitTileInfo conduitInfo;

	public ResourceConduitTile(HexCoords coords, float height, ResourceConduitTileInfo tInfo) : base(coords, height, tInfo)
	{
		conduitInfo = tInfo;
	}

	public override void OnHeightChanged()
	{
		base.OnHeightChanged();
	}

	public override void OnPlaced()
	{
		base.OnPlaced();
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
	}

	public override Entity Render()
	{
		return base.Render();
	}

	public override void TileUpdated(Tile src, TileUpdateType updateType)
	{
		base.TileUpdated(src, updateType);
	}
}