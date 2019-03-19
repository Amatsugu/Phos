using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/TileInfo")]
public class TileInfo : MeshEntity
{
	public TileRenderer[] renderers;

}
