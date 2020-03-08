using UnityEngine;

public static class Cartographer
{
	public static Color[] RenderChunk(Map.Chunk chunk, int res = 4)
	{
		int size = Map.Chunk.SIZE * res;
		var colors = new Color[size * size];
		for (int y = 0; y < size; y += res)
		{
			for (int x = 0; x < size; x += res)
			{
				int cX = x / res, cY = y / res;
				var tile1 = chunk.Tiles[cX + cY * Map.Chunk.SIZE];
				var color = tile1.info.material.color;
				if (!tile1.IsUnderwater && cY - 1 >= 0)
				{
					var tile2 = chunk.Tiles[cX + (cY - 1) * Map.Chunk.SIZE];
					var tileDelta = tile1.Height - tile2.Height;
					if (tileDelta < -.3f)
						color *= 1.2f;
					else if (tileDelta > .3f)
						color *= .8f;
				}
				else
				{
					var d = MathUtils.Remap(tile1.Height, 0, tile1.SurfacePoint.y, 1, 0);
					var g = new Gradient();
					g.SetKeys(new GradientColorKey[]
					{
						new GradientColorKey(new Color(66/255f, 209/255f, 245/255f), 0),
						new GradientColorKey(new Color(0, 11/255f, 69/255f), 1)
					}, new GradientAlphaKey[]
					{
						new GradientAlphaKey(1, 1)
					});
					color = Color.Lerp(color, g.Evaluate(d), d);
				}
				color.a = 1;
				for (int pX = x; pX < x + res; pX++)
				{
					for (int pY = y; pY < y + res; pY++)
					{
						colors[pX + pY * size] = color;
					}
				}
			}
		}

		return colors;
	}

	public static Color[] RenderMap(Map map, int res = 4)
	{
		var waterGrad = new Gradient();
		waterGrad.SetKeys(new GradientColorKey[]
		{
			new GradientColorKey(new Color(66/255f, 209/255f, 245/255f), 0),
			new GradientColorKey(new Color(0, 11/255f, 69/255f), 1)
		}, new GradientAlphaKey[]
		{
			new GradientAlphaKey(1, 1)
		});
		var width = map.totalWidth * res;
		var colors = new Color[width * map.totalHeight * res];
		for (int y = 0; y < map.totalHeight; y++)
		{
			for (int x = 0; x < map.totalWidth; x++)
			{
				var tile1 = map[HexCoords.FromOffsetCoords(x, y, map.tileEdgeLength)];
				var color = Color.magenta;
				if (tile1.info.material != null)
					color = tile1.info.material.color;
				if (!tile1.IsUnderwater && y - 1 >= 0)
				{
					var tile2 = map[HexCoords.FromOffsetCoords(x, y - 1, map.tileEdgeLength)];
					var tileDelta = tile1.Height - tile2.Height;
					if (tileDelta < -.3f)
						color *= 1.2f;
					else if (tileDelta > .3f)
						color *= .8f;
				}
				else
				{
					var d = MathUtils.Remap(tile1.Height, 0, tile1.SurfacePoint.y, 1, 0);

					color = Color.Lerp(color, waterGrad.Evaluate(d), d);
				}
				color.a = 1;
				for (int pX = x * res; pX < (x + 1) * res; pX++)
				{
					for (int pY = y * res; pY < (y + 1) * res; pY++)
					{
						colors[pX + pY * width] = color;
					}
				}
			}
		}

		return colors;
	}
}