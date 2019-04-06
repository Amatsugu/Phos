using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class BuildingTile : Tile
{
	public BuildingTileInfo buildingInfo;

	private Entity _building;

	public BuildingTile(HexCoords coords, float height, BuildingTileInfo tInfo = null) : base(coords, height, tInfo)
	{
		buildingInfo = tInfo;
	}

	public override Entity Render()
	{
		if(buildingInfo.buildingMesh != null)
			_building = buildingInfo.buildingMesh.Instantiate(SurfacePoint);
		return base.Render();
	}

	public override void OnHeightChanged()
	{
		base.OnHeightChanged();
		if (buildingInfo.buildingMesh != null)
			Map.EM.SetComponentData(_building, new Translation { Value = SurfacePoint });
	}

	public override void Show(bool isShown)
	{
		base.Show(isShown);
		if (buildingInfo.buildingMesh == null)
			return;
		if (isShown)
			Map.EM.RemoveComponent(_building, typeof(FrozenRenderSceneTag));
		else
			Map.EM.AddComponent(_building, typeof(FrozenRenderSceneTag));
	}
}

public class PoweredBuildingTile : BuildingTile
{
	public PoweredBuildingTile(HexCoords coords, float height, BuildingTileInfo tInfo = null) : base(coords, height, tInfo)
	{
	}

	public override void OnPlaced()
	{
		base.OnPlaced();
		var neightbors = Map.ActiveMap.GetNeighbors(Coords);
		for (int i = 0; i < 6; i++)
		{
			if (neightbors[i] is ConnectedTile t)
				t.UpdateConnections();
		}
	}
}
