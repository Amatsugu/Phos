using System.Collections.Generic;
using System.Linq;

using Unity.Entities;

using UnityEngine;

namespace Amatsugu.Phos
{
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
					if(tileDef.tile.decorators != null)
						DeclareDecorators(tileDef.tile.decorators);

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
