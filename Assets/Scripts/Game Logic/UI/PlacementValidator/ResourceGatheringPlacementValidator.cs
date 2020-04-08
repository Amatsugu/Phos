using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Validators/Resource Gathering Placement Validator")]
public class ResourceGatheringPlacementValidator : PlacementValidator
{
	public MeshEntity gatheringIndicator;
	public MeshEntity cannotGatherIndicator;
	private Dictionary<int, int> _resInRange = new Dictionary<int, int>();
	private Dictionary<int, List<Tile>> _resTiles = new Dictionary<int, List<Tile>>();

	public override bool ValidatePlacement(Map map, HexCoords pos, BuildingTileEntity buildingTile, IndicatorManager indicatorManager)
	{
		var buildingInfo = buildingTile as ResourceGatheringBuildingEntity;
		if (buildingInfo == null)
			throw new System.Exception();
		_resInRange.Clear();
		_resTiles.Clear();
		//Find tiles in range
		map.HexSelectForEach(pos, buildingInfo.size + buildingInfo.gatherRange, t =>
		{
			if(t is ResourceTile rt && !rt.gatherer.isCreated)
			{
				var yeild = rt.resInfo.resourceYields;
				for (int i = 0; i < yeild.Length; i++)
				{
					var yID = yeild[i].id;
					if (_resInRange.ContainsKey(yID))
					{
						_resInRange[yID]++;
						_resTiles[yID].Add(t);
					}
					else
					{
						_resInRange.Add(yID, 1);
						_resTiles.Add(yID, new List<Tile> { t });
					}
				}
			}
		}, false);

		bool hasRes = false;
		var gatherString = new StringBuilder();
		//Find gatherable resources
		for (int i = 0; i < buildingInfo.resourcesToGather.Length; i++)
		{
			var res = buildingInfo.resourcesToGather[i];
			if (!_resInRange.ContainsKey(res.id))
				continue;
			var gatherAmmount = Mathf.FloorToInt(_resInRange[res.id] * res.ammount);
			gatherString.AppendLine($"+{gatherAmmount}{ResourceDatabase.GetResourceString(res.id)}");
			var tiles = _resTiles[res.id];
			for (int j = 0; j < tiles.Count; j++)
			{
				hasRes = true;
				indicatorManager.SetIndicator(tiles[j], gatheringIndicator);
			}
			_resTiles.Remove(res.id);
		}
		indicatorManager.floatingText.SetText(gatherString);
		indicatorManager.floatingText.gameObject.SetActive(true);
		indicatorManager.floatingText.rectTransform.position = GameRegistry.Camera.WorldToScreenPoint(map[pos].SurfacePoint) + new Vector3(0, 15);
		bool cannotGather = _resInRange.Count > 0;
		bool cannotPlace = false;
		var tilesToOccupy = HexCoords.SpiralSelect(pos, buildingTile.size, innerRadius: map.innerRadius);
		foreach (var tiles in _resTiles)
		{
			for (int i = 0; i < tiles.Value.Count; i++)
			{
				for (int j = 0; j < tilesToOccupy.Length; j++)
				{
					if(tiles.Value[i].Coords == tilesToOccupy[j])
					{
						cannotPlace = true;
						indicatorManager.SetIndicator(tiles.Value[i], errorIndicator);
					}
					else
						indicatorManager.SetIndicator(tiles.Value[i], cannotGatherIndicator);
				}

			}
		}
		if (!hasRes)
		{
			if (cannotGather)
				indicatorManager.LogError("Cannot gatther these resources");
			else
				indicatorManager.LogError("No resources to gather");
		}
		if (cannotPlace)
			indicatorManager.LogError("Cannot place on these tiles");

		return (hasRes && !cannotPlace) && base.ValidatePlacement(map, pos, buildingTile, indicatorManager);
	}
}
