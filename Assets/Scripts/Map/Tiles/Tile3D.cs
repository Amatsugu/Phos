using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class Tile3D : Tile
{

	public Tile3D(HexCoords coords, float height, TileInfo tInfo) : base(coords)
	{
		Height = height;
		info = tInfo;
		SurfacePoint = new Vector3(coords.WorldX, height, coords.WorldZ);
	}

	public override void Destroy()
	{
		if(_tileEntity == null)
			_tileObject = null;
		else
			_entityManager.DestroyEntity(_tileEntity);
	}

	public override void Render(Transform parent)
	{
		isShown = true;
		var pos = Coords.WorldXZ;
		_tileObject = Object.Instantiate(info.tilePrefab, pos, Quaternion.identity, parent);
		_tileObject.AddComponent<WorldTile>().coord = Coords;
		_tileObject.transform.localScale = new Vector3(1, Height, 1);
		_tileObject.name = $"{info.name} : {Coords}";
	}

	public override void TileClicked()
	{

	}

	public override void Render(EntityManager entityManager)
	{
		_entityManager = entityManager;
		_tileEntity = entityManager.Instantiate(info.GetEntity(entityManager));
		//entityManager.SetName(_curEntity, $"{info.name} : {Coords}");
		entityManager.SetComponentData(_tileEntity, new Translation { Value = Coords.WorldXZ });
		entityManager.SetComponentData(_tileEntity, new NonUniformScale { Value = new Vector3(1, Height, 1) });
	}
}
