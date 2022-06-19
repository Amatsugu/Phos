using Amatsugu.Phos.TileEntities;

using System.Collections.Generic;
using System.Linq;

using Unity.Entities;

using UnityEngine;

namespace Amatsugu.Phos
{
	/// <summary>
	/// Declare prefab references
	/// </summary>
	[UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
	public class PrefabDeclarationSystem : GameObjectConversionSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((MapAuthoring m) =>
			{
				var mapEntity = GetPrimaryEntity(m);
				var database = m.tileDatabase;
				var tiles = database.tileEntites.Values.ToArray();
				for (int i = 0; i < tiles.Length; i++)
				{
					var tileDef = tiles[i];
					if (tileDef.tile.decorators != null)
						DeclareDecorators(tileDef.tile.decorators);

					if (tileDef.tile is BuildingTileEntity b && b.validator != null)
					{
						var indicators = b.validator.GetIndicatorPrefabs();
						for (int j = 0; j < indicators.Count; j++)
							DeclareReferencedPrefab(indicators[j]);
					}

					if (tileDef.tile.tilePrefab == null)
						continue;

					DeclareReferencedPrefab(tileDef.tile.tilePrefab);
				}
			});

			Entities.ForEach((ConduitLinesAuthoring lines) =>
			{
				DeclareReferencedPrefab(lines.activeLine);
				DeclareReferencedPrefab(lines.inactiveLine);
			});
			Entities.ForEach((InitializeWeather weather) =>
			{
				DeclareReferencedPrefab(weather.cloudPrefab);
			});

			var buildings = GameRegistry.BuildingDatabase.buildings.Values.ToArray();
			for (int i = 0; i < buildings.Length; i++)
			{
				var building = buildings[i];
				if (building.info.buildingPrefab == null)
					continue;
				DeclareReferencedPrefab(building.info.buildingPrefab);
			}

			var units = GameRegistry.UnitDatabase.unitEntites.Values.ToArray();
			for (int i = 0; i < units.Length; i++)
			{
				var unit = units[i];
				DeclareReferencedPrefab(unit.info.unitPrefab);
			}

			var prefabs = GameRegistry.PrefabsToInit;
			for (int i = 0; i < prefabs.Count; i++)
			{
				var prefab = prefabs[i];
				DeclareReferencedPrefab(prefab);
			}
		}

		private void DeclareDecorators(TileDecorator[] tileDecorators)
		{
			var objects = new List<GameObject>();
			for (int i = 0; i < tileDecorators.Length; i++)
				tileDecorators[i].DeclarePrefabs(objects);
			for (int i = 0; i < objects.Count; i++)
			{
				DeclareReferencedPrefab(objects[i]);
			}
		}
	}
}