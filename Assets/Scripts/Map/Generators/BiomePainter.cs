using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile Mapper/Biome Painter")]
public class BiomePainter : ScriptableObject
{
	[SerializeField]
	public TileMapper[] biomes;


	public int[] GetBiomeMap()
	{
		return null;
	}

	public float[] GetMoistureMap(int width, int height, float[] heightMap, float min, float max, float seaLevel)
	{
		var moistureMap = new float[width * height];
		for (int i = 0; i < moistureMap.Length; i++)
		{
			var h = heightMap[i];
			var m = h <= seaLevel ? MathUtils.Map(seaLevel - h, min, seaLevel, 2, 3) : (Mathf.Pow(MathUtils.Map(h - seaLevel, 0, max, 0, 1), 2) * 3);
			moistureMap[i] = Mathf.Clamp(m, 0, 3);
		}

		return moistureMap;
	}

	public float[] GetTempMap(int width, int height, float[] heightMap, float min, float max, float seaLevel)
	{
		var tempMap = new float[width * height];
		var equator = Random.Range(0, height);
		int maxD = (height/2 < equator) ? equator : Mathf.Abs(height - equator);
		for (int z = 0; z < height; z++)
		{
			for (int x = 0; x < width; x++)
			{
				var d = Mathf.Abs(z - equator);
				var tE = 3-(Mathf.Pow(MathUtils.Map(d, 0, maxD, 0, 1), 2) * 3);
				var h = heightMap[x + z * width];
				var tH = Mathf.Clamp(MathUtils.Map(h, 0, max, 0, 3), 0, 3);
				tH = 3 - tH;
				var t = Mathf.Clamp(value: (tH + (tE* 3)) / 4f, min: 0, max: 3);
				tempMap[x + z * width] = t;
			}
		}
		return tempMap;
	}


	public (TileInfo tInfo, int biomeId) GetTile(float moisture, float temp, float height, float seaLevel)
	{
		var biome = Mathf.RoundToInt(temp) + Mathf.RoundToInt(moisture) * 4;
		return (biomes[biome].GetTile(height, seaLevel), biome);
	}

}
