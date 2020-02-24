using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Cartographer
{
    public static Color[] RenderChunk(Map.Chunk chunk, int res = 512)
    {
        int size = Map.Chunk.SIZE * res;
        var colors = new Color[size * size];
        for (int y = 0; y < size; y+= res)
        {
            for (int x = 0; x < size; x+= res)
            {
                int cX = x / res, cY = y / res;
                var tile = chunk.Tiles[cX * cY * Map.Chunk.SIZE];
                for (int pX = 0; pX  < x + res; pX ++)
                {
                    for (int pY = 0; pY < y + res; pY++)
                    {
                        colors[x + y * size] = tile.info.material.color;
                    }
                }
            }
        }

        return colors;
    }
}
