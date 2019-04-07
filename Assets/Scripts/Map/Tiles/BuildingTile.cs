using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class BuildingTile : Tile
{
	public BuildingTileInfo buildingInfo;

	public bool HasHQConnection { get; protected set; }

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

	public virtual void OnHQConnected()
	{
		if (HasHQConnection)
			return;
		HasHQConnection = true;
		var neighbors = Map.ActiveMap.GetNeighbors(Coords);
		for (int i = 0; i < 6; i++)
		{
			if (neighbors[i] is PoweredBuildingTile b)
				b.OnHQConnected();
			else if (neighbors[i] is ConnectedTile t)
				t.OnHQConnected();
		}
	}

	public virtual void OnHQDisconnected()
	{
		if (!HasHQConnection)
			return;
		HasHQConnection = false;
		var neighbors = Map.ActiveMap.GetNeighbors(Coords);
		for (int i = 0; i < 6; i++)
		{
			if (neighbors[i] is PoweredBuildingTile b)
				b.OnHQDisconnected();
			else if (neighbors[i] is ConnectedTile t)
				t.OnHQDisconnected();
		}
	}

	public override void Destroy()
	{
		base.Destroy();
		if (buildingInfo.buildingMesh != null)
			Map.EM.DestroyEntity(_building);
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
		Debug.Log($"{buildingInfo.name} Placed");
		var neightbors = Map.ActiveMap.GetNeighbors(Coords);
		for (int i = 0; i < 6; i++)
		{
			if (neightbors[i] is ConnectedTile t)
				t.UpdateConnections();
			if (neightbors[i] is BuildingTile b && b.HasHQConnection)
				OnHQConnected();
		}
		if (!HasHQConnection)
			OnHQDisconnected();
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		OnHQDisconnected();	
	}
}
