using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile3D : Tile
{
	public float Height { get; protected set; }
	public Vector3 SurfacePoint { get; private set; }



	public Tile3D(HexCoords coords, float height, TileInfo tInfo) : base(coords)
	{
		Height = height;
		info = tInfo;
		SurfacePoint = new Vector3(coords.WorldX, height, coords.WorldZ);
	}

	public override void Destroy()
	{
		_tileObject = null;
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

	

	public void UpdateHeight(float height)
	{
		_tileObject.transform.localScale = new Vector3(1, Height = height, 1);
		SurfacePoint = new Vector3(Coords.WorldX, height, Coords.WorldZ);
	}

	public override void TileClicked()
	{

	}
}
