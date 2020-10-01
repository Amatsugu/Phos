using Amatsugu.Phos.TileEntities;
using Amatsugu.Phos.Units;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

using UnityEngine;
using UnityEngine.UIElements;

namespace Amatsugu.Phos.Tiles
{
	public class FactoryBuildingTile : PoweredBuildingTile
	{
		public FactoryTileEntity factoryInfo;
		private MobileUnitEntity _curUnit;
		private BuildPhysicsWorld _physicsWorld;
		public FactoryBuildingTile(HexCoords coords, float height, Map map, FactoryTileEntity tInfo, int rotation) : base(coords, height, map, tInfo, rotation)
		{
			factoryInfo = tInfo;
			_physicsWorld = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
		}

		public virtual void StartConstruction(MobileUnitEntity unitEntity)
		{
			Debug.Log($"Unit starting construction: {unitEntity.GetNameString()}");
			unitEntity.constructionMesh.Instantiate(SurfacePoint + new float3(0, .3f, 0), Quaternion.identity, unitEntity, 0.8f, unitEntity.buildTime);
			_curUnit = unitEntity;
		}

		public virtual void FinishConstruction()
		{
			var unit = GameRegistry.GameMap.AddUnit(_curUnit, this, factoryInfo.faction);
			unit.MoveTo(FindEmptyTile().WorldPos);
			_curUnit = null;
			GameEvents.InvokeOnUnitBuilt(Coords);
		}


		private HexCoords FindEmptyTile()
		{
			var curR = 1;
			var hits = new NativeList<int>(Allocator.Temp);
			while(true)
			{
				var curRing = HexCoords.SelectRing(Coords, curR);
				for (int i = 0; i < curRing.Length; i++)
				{
					hits.Clear();
					var pos = curRing[i].WorldPos;
					PhysicsUtilz.AABBCast(_physicsWorld, new Aabb
					{
						Min = pos - new float3(-.5f, 0, -.5f),
						Max = pos + new float3(.5f, 50, .5f)
					}, new CollisionFilter
					{
						CollidesWith = (uint)CollisionLayer.Building,
						BelongsTo = (uint)CollisionLayer.Unit
					}, ref hits);

					if (hits.Length == 0)
					{
						hits.Dispose();
						return curRing[i];
					}
				}
				curR++;
			}
		}

	}
}