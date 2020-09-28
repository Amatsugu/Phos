using Amatsugu.Phos;
using Amatsugu.Phos.Tiles;

using AnimationSystem.Animations;
using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Building Decorators/Rotating")]
public class RotatingDecorator : TileDecorator
{
    public float3 rotationAxis = new float3(0,1,0);

    public float rotateSpeed;

    public override int GetDecorEntityCount(Tile tile) => 1;

    public override void Render(Tile tile, NativeSlice<Entity> decor)
    {
        decor[0] = meshEntity.Instantiate(tile.SurfacePoint);
        Map.EM.AddComponentData(decor[0], new RotateAxis { Value = rotationAxis });
        Map.EM.AddComponentData(decor[0], new RotateSpeed { Value = math.radians(rotateSpeed) });
    }
}
