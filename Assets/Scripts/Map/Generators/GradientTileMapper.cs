using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile Mapper/Gradient")]
public class GradientTileMapper : TileMapper
{
	public Gradient tileGradient = new Gradient();
	public Gradient oceanGradient = new Gradient();

	public AnimationCurve moveCostCurve = new AnimationCurve();
	public TileInfo[] Tiles;
	public TileInfo oceanTile;
	public Dictionary<Color, TileInfo> tiles = new Dictionary<Color, TileInfo>();


	public void OnEnable()
	{
		tiles.Clear();
		tileGradient.mode = GradientMode.Fixed;
		for (int i = 0; i < Tiles.Length; i++)
		{
			if (i > Tiles.Length - 1)
				tiles.Add(tileGradient.colorKeys[i].color, null);
			else
				tiles.Add(tileGradient.colorKeys[i].color, Tiles[i]);
		}
	}

	public override TileInfo GetTile(float sample, float seaLevel, float maxValue = 1)
	{
		if (sample <= seaLevel)
			return oceanTile;
		sample = (sample - seaLevel) / (maxValue - seaLevel);
		return tiles[tileGradient.Evaluate(sample)];
	}
}
