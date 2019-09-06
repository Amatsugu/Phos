using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Vent Tile Info")]
public class VentTileInfo : ResourceTileInfo
{
	[Header("Vent")]
	public MeshEntityRotatable core;
	public MeshEntityRotatable shell;

	public override Tile CreateTile(HexCoords pos, float height)
	{
		return new GeothermalVentTile(pos, height, this);
	}
}
