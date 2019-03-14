using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile3D : Tile
{
	public float Height { get; protected set; }
	public Vector3 SurfacePoint { get; }
	public TileInfo info;



	public Tile3D(HexCoords coords, float height, TileInfo tInfo) : base(coords)
	{
		Height = height;
		info = tInfo;
		SurfacePoint = new Vector3(coords.WorldX, height, coords.WorldZ);
	}

	public override void DestroyTile()
	{
		Object.Destroy(_tileObject);
	}

	public override void RenderTile(Transform parent)
	{
		isShown = true;
		var pos = new Vector3(Coords.WorldX, 0, Coords.WorldZ);
		_tileObject = Object.Instantiate(info.tilePrefab, pos, Quaternion.identity, parent);
		_tileObject.AddComponent<WorldTile>().coord = Coords;
		_tileObject.transform.localScale = new Vector3(1, Height, 1);
		_tileObject.name = $"{info.name} : {Coords}";
	}

	

	public void UpdateHeight(float height)
	{
		_tileObject.transform.localScale = new Vector3(1, Height = height, 1);
	}

	public override void TileClicked()
	{

	}
}
