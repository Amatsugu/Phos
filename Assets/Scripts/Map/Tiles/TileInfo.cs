using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Tile Info")]
public class TileInfo : MeshEntity
{
    public TileDecorator[] decorators;

	protected override EntityArchetype GetArchetype(bool localToParent = false)
	{
		return Map.EM.CreateArchetype(
				typeof(Translation),
				localToParent ? typeof(LocalToParent) : typeof(LocalToWorld),
				typeof(NonUniformScale),
				typeof(RenderMesh),
				typeof(HexPosition)
				);
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
