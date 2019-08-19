using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Tile Info")]
public class TileInfo : MeshEntity
{
	[Header("Tile Info")]
	public string description;
    public TileDecorator[] decorators;
	public bool isTraverseable = true;

	public override IEnumerable<ComponentType> GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[]{
			typeof(HexPosition)
		});
	}

	public virtual Entity Instantiate(HexCoords pos, Vector3 scale)
	{
		var e = Instantiate(pos.worldXZ, scale);
		Map.EM.SetComponentData(e, new HexPosition { coords = pos });
		return e;
	}

	public virtual Tile CreateTile(HexCoords pos, float height)
	{
		return new Tile(pos, height, this);
	}
}
