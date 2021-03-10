using Amatsugu.Phos.Tiles;

using System.Collections;
using System.Collections.Generic;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

namespace Amatsugu.Phos.ECS
{
    public class TileDefination : ScriptableObject
    {
        public string description;
        public bool isTraverseable = true;
        public GameObject tileMeshPrefab;

        public virtual Tile CreateTile(Map map, HexCoords pos, float height)
		{
            return new Tile(map, pos, height, this);
		}

        public virtual Entity Instantiate(HexCoords coords, float height)
		{
            var e = GameObjectConversionUtility.ConvertGameObjectHierarchy(tileMeshPrefab, null);
            Map.EM.SetComponentData(e, new Translation { Value = new float3(coords.WorldPos.x, height, coords.WorldPos.z) });
            Map.EM.SetComponentData(e, new HexPosition { Value = coords });

            return e;
        }
    }
}
