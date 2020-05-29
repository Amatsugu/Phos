using Amatsugu.Phos.Tiles;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile Decorators/Mesh")]
public class MeshDecorator : TileDecorator
{
	public Vector3 rotation;
	public float3 offset;
	public float randomRotMin = 0;
	public float randomRotMax = 180;

	public override int GetDecorEntityCount(Tile tile)
	{
		return 1;
	}

	/*public override Matrix4x4[] GetTransformMatricies(Tile tile)
	{
		var rot = rotation;
		rot.y = Mathf.PerlinNoise(tile.Coords.worldX / 10f, tile.Coords.worldZ / 10f).Remap(0,1, randomRotMin, randomRotMax);
		var qRot = Quaternion.Euler(rot);
		var transforms = new Matrix4x4[GetDecorEntityCount(tile)];
		for (int i = 0; i < transforms.Length; i++)
		{
			transforms[i] = Matrix4x4.TRS(tile.SurfacePoint + offset, qRot, Vector3.one);
		}
		return transforms;
	}
	*/

	public override Entity[] Render(Tile tile)
	{
		var rot = rotation;
		rot.y = Mathf.PerlinNoise(tile.Coords.world.x / 10f, tile.Coords.world.z / 10f).Remap(0, 1, randomRotMin, randomRotMax);
		var e = meshEntity.Instantiate(tile.SurfacePoint + offset, Vector3.one, Quaternion.Euler(rot));
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