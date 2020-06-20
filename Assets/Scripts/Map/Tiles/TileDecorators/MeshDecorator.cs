using Amatsugu.Phos.Tiles;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile Decorators/Mesh")]
public class MeshDecorator : TileDecorator
{
	public bool multiMesh = false;
	[ConditionalHide("multiMesh")]
	public MeshEntityRotatable[] meshEntities;
	public Vector3 rotation;
	public float3 offset;
	public float randomRotMin = 0;
	public float randomRotMax = 180;

	public override int GetDecorEntityCount(Tile tile)
	{
		return 1;
	}

	public override Entity[] Render(Tile tile)
	{
		var rot = rotation;
		var mesh = meshEntity;
		if (multiMesh)
			mesh = meshEntities[UnityEngine.Random.Range(0, meshEntities.Length)];
		rot.y = Mathf.PerlinNoise(tile.Coords.WorldPos.x / 10f, tile.Coords.WorldPos.z / 10f).Remap(0, 1, randomRotMin, randomRotMax);
		var e = mesh.Instantiate(tile.SurfacePoint + offset, Vector3.one, Quaternion.Euler(rot));
		return new Entity[] { e };
	}

	public override void UpdateHeight(NativeSlice<Entity> decor, Tile tile)
	{
		foreach (var tileDecor in decor)
		{
			var p = Map.EM.GetComponentData<Translation>(tileDecor);
			p.Value.y = tile.Height + offset.y;
			Map.EM.SetComponentData(tileDecor, p);
		}
	}
}