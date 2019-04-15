using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ConnectedTile : PoweredBuildingTile
{
	public ConnectedTileInfo connectedTileInfo;

	private NativeArray<Entity> _connections;

	public ConnectedTile(HexCoords coords, float height, ConnectedTileInfo tInfo = null) : base(coords, height, tInfo)
	{
		connectedTileInfo = tInfo;
		_connections = new NativeArray<Entity>(6, Allocator.Persistent);
	}

	public override void TileUpdated(Tile src, TileUpdateType updateType)
	{
		base.TileUpdated(src, updateType);
		UpdateConnections();
	}

	public override void OnPlaced()
	{
		for (int i = 0; i < 6; i++)
			_connections[i] = connectedTileInfo.connectionMesh.Instantiate(SurfacePoint, Vector3.one, Quaternion.Euler(new Vector3(0, (i * 60) - 90, 0)));
		base.OnPlaced();
		UpdateConnections();
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

	public override void OnHQConnected(PoweredBuildingTile src)
	{
		base.OnHQConnected(src);
		UpdateConnections();
	}

	public override void OnHQDisconnected(PoweredBuildingTile src, HashSet<Tile> visited, bool verified = false)
	{
		base.OnHQDisconnected(src, visited, verified);
		UpdateConnections();
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
			Map.EM.RemoveComponent(_connections, typeof(Frozen));
		else
			Map.EM.AddComponent(_connections, typeof(Frozen));
	}

	public void UpdateConnections()
	{
		if (!HasHQConnection)
		{
			for (int i = 0; i < 6; i++)
			{
				if (!Map.EM.HasComponent<Disabled>(_connections[i]))
					Map.EM.AddComponent(_connections[i], typeof(Disabled));
			}
			return;
		}
		var neighbors = Map.ActiveMap.GetNeighbors(Coords);
		for (int i = 0; i < neighbors.Length; i++)
		{
			var n = neighbors[i];
			if (n == null)
				continue;
			if (n is PoweredBuildingTile pb && pb.HasHQConnection)
			{
				if(Map.EM.HasComponent<Disabled>(_connections[i]))
					Map.EM.RemoveComponent(_connections[i], typeof(Disabled));
			}else
			{
				if (!Map.EM.HasComponent<Disabled>(_connections[i]))
					Map.EM.AddComponent(_connections[i], typeof(Disabled));
			}
		}
	}
}
