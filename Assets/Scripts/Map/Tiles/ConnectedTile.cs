using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ConnectedTile : BuildingTile
{
	public ConnectedTileInfo connectedTileInfo;

	private NativeArray<Entity> _connections;

	public ConnectedTile(HexCoords coords, float height, ConnectedTileInfo tInfo = null) : base(coords, height, tInfo)
	{
		connectedTileInfo = tInfo;
		_connections = new NativeArray<Entity>(6, Allocator.Persistent);
		for (int i = 0; i < 6; i++)
		{
			_connections[i] = connectedTileInfo.connectionMesh.Instantiate(SurfacePoint, Vector3.one, Quaternion.Euler(new Vector3(0, (i * 60) - 90, 0)));
			//Map.EM.AddComponent(_connections[i], typeof(ChildOf));
			//Map.EM.AddComponent(_connections[i], typeof(LocalTranslation));
			//Map.EM.SetComponentData(_connections[i], new ChildOf { parent = _tileEntity });
			//Map.EM.SetComponentData(_connections[i], new LocalTranslation { position = SurfacePoint});
		}
	}

	public override void OnPlaced()
	{
		base.OnPlaced();
		UpdateConnections();
		var neighbors = Map.ActiveMap.GetNeighbors(Coords);
		for (int i = 0; i < neighbors.Length; i++)
		{
			if (neighbors[i] is ConnectedTile t)
				t.UpdateConnections();
		}
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		var neighbors = Map.ActiveMap.GetNeighbors(Coords);
		for (int i = 0; i < neighbors.Length; i++)
		{
			if (neighbors[i] is ConnectedTile t)
				t.UpdateConnections();
		}
	}

	public override void OnHeightChanged()
	{
		base.OnHeightChanged();
		for (int i = 0; i < 6; i++)
		{
			var t = Map.EM.GetComponentData<Translation>(_connections[i]);
			t.Value.y = Height;
			Map.EM.SetComponentData(_connections[i], t);
		}
	}

	public override void Destroy()
	{
		base.Destroy();
		try
		{
			Map.EM.DestroyEntity(_connections);
		}catch
		{

		}
		finally
		{
			_connections.Dispose();
		}
	}

	public override void Show(bool isShown)
	{
		base.Show(isShown);
		if (isShown)
			UpdateConnections();
		else
		{
			for (int i = 0; i < 6; i++)
			{
				if (!Map.EM.HasComponent<FrozenRenderSceneTag>(_connections[i]))
					Map.EM.AddComponent(_connections[i], typeof(FrozenRenderSceneTag));
			}
		}
	}

	public void UpdateConnections()
	{
		var neighbors = Map.ActiveMap.GetNeighbors(Coords);
		for (int i = 0; i < neighbors.Length; i++)
		{
			var n = neighbors[i];
			if (n == null)
				continue;
			if (n is ConnectedTile || n is PoweredBuildingTile || n is HQTile)
			{
				if(Map.EM.HasComponent<FrozenRenderSceneTag>(_connections[i]))
					Map.EM.RemoveComponent(_connections[i], typeof(FrozenRenderSceneTag));
			}else
			{
				if (!Map.EM.HasComponent<FrozenRenderSceneTag>(_connections[i]))
					Map.EM.AddComponent(_connections[i], typeof(FrozenRenderSceneTag));
			}
		}
	}
}
