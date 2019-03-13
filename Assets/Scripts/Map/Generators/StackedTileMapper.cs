using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile Mapper/Stacked")]
public class StackedTileMapper : TileMapper
{
	[System.Serializable]
	public class TileLayer
	{
		public TileInfo tileInfo;
		public float startHeight;
	}

	public TileLayer[] surfaceLayers;
	public TileLayer[] oceanLayers;

	public override TileInfo GetTile(float sample, float seaLevel, float maxValue = 1)
	{
		TileInfo tile = null;
		sample -= seaLevel;
		if (sample < 0)
		{
			//sample = seaLevel - sample;
			foreach (var tileLayer in oceanLayers)
			{
				if (sample <= tileLayer.startHeight)
					tile = tileLayer.tileInfo;
			}
		}
		else
		{
			foreach (var tileLayer in surfaceLayers)
			{
				if (sample >= tileLayer.startHeight)
					tile = tileLayer.tileInfo;
			}
		}
		return tile;
	}
}
