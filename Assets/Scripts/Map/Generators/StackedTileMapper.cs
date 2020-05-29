using Amatsugu.Phos.TileEntities;

using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile Mapper/Stacked")]
public class StackedTileMapper : TileMapper
{
	[System.Serializable]
	public class TileLayer
	{
		public TileEntity tileInfo;
		public float startHeight;
	}

	public TileLayer[] surfaceLayers;
	public TileLayer[] oceanLayers;

	public override TileEntity GetTile(float altitude, float seaLevel, float maxValue = 1)
	{
		TileEntity tile = null;
		altitude -= seaLevel;
		if (altitude < 0)
		{
			//sample = seaLevel - sample;
			foreach (var tileLayer in oceanLayers)
			{
				if (altitude <= tileLayer.startHeight)
					tile = tileLayer.tileInfo;
			}
		}
		else
		{
			foreach (var tileLayer in surfaceLayers)
			{
				if (altitude >= tileLayer.startHeight)
					tile = tileLayer.tileInfo;
			}
		}
		return tile;
	}
}