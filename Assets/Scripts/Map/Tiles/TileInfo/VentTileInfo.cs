using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Vent Tile Info")]
public class VentTileInfo : ResourceTileInfo
{
	[Header("Vent")]
	public MeshEntityRotatable core;

	public MeshEntityRotatable shell;
	public GameObject gyser;

	public override Tile CreateTile(Map map, HexCoords pos, float height)
	{
		return new GeothermalVentTile(pos, height, map, this);
	}
}