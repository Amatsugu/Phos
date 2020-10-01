using System.Collections;
using System.Collections.Generic;

using Unity.Physics;

using UnityEngine;

namespace Amatsugu.Phos
{
    public class CollisionMaskTests : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var playerBuilding = new CollisionFilter
            {
                BelongsTo = (uint)(CollisionLayer.Building | CollisionLayer.Player),
                CollidesWith = ~0u
            };

            var phosBuilding = new CollisionFilter
            {
                BelongsTo = (uint)(CollisionLayer.Building | CollisionLayer.Phos),
                CollidesWith = ~0u
            };

            var playerProj = new CollisionFilter
            {
                BelongsTo = (uint)CollisionLayer.Projectile,
                CollidesWith = (uint)CollisionLayer.Phos
            };

            var phosProj = new CollisionFilter
            {
                BelongsTo = (uint)CollisionLayer.Projectile,
                CollidesWith = (uint)CollisionLayer.Player
            };

            Debug.Log($"Player Building -> Player Proj: {CollisionFilter.IsCollisionEnabled(playerBuilding, playerProj)}");
            Debug.Log($"Player Building -> Phos Proj: {CollisionFilter.IsCollisionEnabled(playerBuilding, phosProj)}");
            Debug.Log($"Phos Building -> Player Proj: {CollisionFilter.IsCollisionEnabled(phosBuilding, playerProj)}");
            Debug.Log($"Phos Building -> Phos Proj: {CollisionFilter.IsCollisionEnabled(phosBuilding, phosProj)}");
            Debug.Log($"Phos Proj -> Phos Proj: {CollisionFilter.IsCollisionEnabled(phosProj, phosProj)}");
            Debug.Log($"Player Proj -> Phos Proj: {CollisionFilter.IsCollisionEnabled(playerProj, phosProj)}");
            Debug.Log(~Faction.Phos);

        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
