﻿using Unity.Entities;
using Unity.Rendering;

using UnityEngine;

public class GeothermalVentTile : ResourceTile
{
	public VentTileInfo ventInfo;

	private Entity _core;
	private GameObject _gyser;

	public GeothermalVentTile(HexCoords coords, float height, VentTileInfo tInfo = null) : base(coords, height, tInfo)
	{
		ventInfo = tInfo;
	}

	public override TileEntity GetMeshEntity()
	{
		return originalTile;
	}

	public override Entity Render()
	{
		_core = ventInfo.core.Instantiate(new Vector3(Coords.worldX, Height, Coords.worldZ));
		_gyser = GameObject.Instantiate(ventInfo.gyser, new Vector3(Coords.worldX, Height, Coords.worldZ), Quaternion.identity);
		return base.Render();
	}

	public override void Show(bool isShown)
	{
		if (IsShown != isShown)
		{
			if (isShown)
				Map.EM.RemoveComponent<FrozenRenderSceneTag>(_core);
			else
				Map.EM.AddComponent(_core, typeof(FrozenRenderSceneTag));
			_gyser.SetActive(isShown);
		}
		base.Show(isShown);
	}

	public override void Destroy()
	{
		GameObject.Destroy(_gyser);
		Map.EM.DestroyEntity(_core);
		base.Destroy();
	}
}

public class GeothermalVentShellTile : ResourceTile
{
	public VentTileInfo ventInfo;
	public float angle;
	public Entity _shell;

	public GeothermalVentShellTile(HexCoords coords, float height, float angle, VentTileInfo tInfo = null) : base(coords, height, tInfo)
	{
		ventInfo = tInfo;
		this.angle = angle;
	}

	public override TileEntity GetMeshEntity()
	{
		return originalTile;
	}

	public override Entity Render()
	{
		_shell = ventInfo.shell.Instantiate(new Vector3(Coords.worldX, Height, Coords.worldZ), Vector3.one, Quaternion.Euler(0, angle - 60, 0));
		return base.Render();
	}

	public override void Show(bool isShown)
	{
		base.Show(isShown);
		if (isShown)
			Map.EM.RemoveComponent<FrozenRenderSceneTag>(_shell);
		else
			Map.EM.AddComponent(_shell, typeof(FrozenRenderSceneTag));
	}

	public override void Destroy()
	{
		base.Destroy();
		Map.EM.DestroyEntity(_shell);
	}
}