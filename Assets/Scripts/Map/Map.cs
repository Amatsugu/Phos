using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using UnityEngine;

public class Map<T> : IEnumerable<T> where T : Tile
{

	public T[] Tiles { get; }
	public int Height { get; }
	public int Width { get; }
	public int Length => Tiles.Length;
	public float TileEdgeLength { get; }
	public float LongDiagonal => 2 * TileEdgeLength;
	public float ShortDiagonal => Mathf.Sqrt(3f) * TileEdgeLength;

	public float InnerRadius => Mathf.Sqrt(3f) / 2f * TileEdgeLength;
	public float SeaLevel;
	public Transform Parent { get; }

	public Map(int height, int width, Transform parent, float edgeLength = 1)
	{
		Height = height;
		Width = width;
		Tiles = new T[height * width];
		TileEdgeLength = edgeLength;
		Parent = parent;
	}

	/// <summary>
	/// Get the Tile at a given index
	/// </summary>
	/// <param name="i">Tile index</param>
	/// <returns>Tile at Index</returns>
	public T this[int i]
	{
		set
		{
			Tiles[i] = value;
		}
		get
		{
			return Tiles[i];
		}
	}

	/// <summary>
	/// Get a tile the given HexCoords position
	/// </summary>
	/// <param name="tile">Position</param>
	/// <returns>Tile at position</returns>
	public T this[HexCoords tile]
	{
		get => Tiles[tile.ToIndex(Width)];
		set => Tiles[tile.ToIndex(Width)] = value;
	}


	/// <summary>
	/// Get a tile at the given HexCoords Position
	/// </summary>
	/// <param name="x">X component</param>
	/// <param name="y">Y component</param>
	/// <param name="z">Z component</param>
	/// <returns>Tile at position</returns>
	public T this[int x, int y, int z]
	{
		get
		{
			if (-x - y != z)
				return null;
			int oX = x + y / 2;
			if (oX < 0 || oX >= Width)
				return null;
			if (y >= Height)
				return null;
			var index = x + y * Width + y / 2;
			if (index < 0 || index > Length)
				return null;
			return Tiles[index];
		}

	}

	internal void UpdateView(Camera cam, Vector3 viewPadding)
	{
		var min = cam.ScreenPointToRay(new Vector3(0, SeaLevel, 0));
		var max = cam.ScreenPointToRay(new Vector3(Screen.width, SeaLevel, Screen.height));
		var l = min.GetPoint(cam.transform.position.y);
		var r = max.GetPoint(cam.transform.position.y);
		var ratio = (float)Screen.width / Screen.height;
		var h = (r.x - l.x);
		var view = new Rect(
			cam.transform.position.x - (h * ratio * .5f) - viewPadding.x,
			cam.transform.position.z - 3,
			(h * ratio) + (2 * viewPadding.x),
			h + (2 * viewPadding.y)
		);
		UpdateView(view);
	}

	public void Render(Transform parent)
	{
		foreach (var tile in Tiles)
			tile?.RenderTile(parent);
	}

	public void UpdateView(Rect view)
	{
		foreach(var tile in Tiles)
		{
			var pos = new Vector2(tile.Coords.WorldX, tile.Coords.WorldZ);
			if (view.Contains(pos))
				tile.Show();
			else
				tile.Hide();
		}

	}

	public T[] GetNeighbors(HexCoords coords)
	{
		T[] neighbors = new T[6];
		neighbors[0] = this[coords.X - 1, coords.Y, coords.Z + 1]; //Left
		neighbors[1] = this[coords.X - 1, coords.Y + 1, coords.Z]; //Top Left
		neighbors[2] = this[coords.X, coords.Y + 1, coords.Z - 1]; //Top Right
		neighbors[3] = this[coords.X + 1, coords.Y, coords.Z - 1]; //Right
		neighbors[4] = this[coords.X + 1, coords.Y - 1, coords.Z]; //Bottom Right
		neighbors[5] = this[coords.X, coords.Y - 1, coords.Z + 1]; //Bottom Left
		return neighbors;
	}


	public List<T> HexSelect(HexCoords center, int radius)
	{
		var selection = new List<T>();
		radius = Mathf.Abs(radius);
		if(radius == 0)
		{
			selection.Add(this[center]);
			return selection;
		}
		for (int y = -radius; y <= radius; y++)
		{
			int xMin = -radius, xMax = radius;
			if (y < 0)
				xMin = -radius - y;
			if (y > 0)
				xMax = radius - y;
			for (int x = xMin; x <= xMax; x++)
			{
				int z = -x - y;
				var t = this[center.X + x, center.Y + y, center.Z + z];
				if(t != null)
					selection.Add(t);
			}
		}
		return selection;
	}

	public List<T> CircularSelect(HexCoords center, float radius)
	{
		var selection = new List<T>();
		radius = Mathf.Abs(radius);
		radius *= InnerRadius;
		if (radius == 0)
		{
			selection.Add(this[center]);
			return selection;
		}
		for (float x = -radius; x < radius; x++)
		{
			for (float z = -radius; z < radius; z++)
			{
				var p = HexCoords.FromPosition(new Vector3(x + center.WorldX, 0, z + center.WorldZ), InnerRadius);
				var d = Mathf.Pow(p.WorldX - center.WorldX, 2) + Mathf.Pow(p.WorldZ - center.WorldZ, 2);
				if(d <= radius * radius)
				{
					var t = this[p]; 
					if(t != null)
						selection.Add(t);
				}
			}
		}
		return selection;
	}

	public enum FlattenMode
	{
		Center,
		Average
	}

	public void CircularFlatten(HexCoords center, float innerRadius, float outerRadius, FlattenMode mode = FlattenMode.Center)
	{
		if (typeof(T) != typeof(Tile3D))
			return;

		var innerSelection = CircularSelect(center, innerRadius).Select(t => t as Tile3D);
		var c = this[center] as Tile3D;
		float height = c.Height;
		if(mode == FlattenMode.Average)
			height = innerSelection.Average(t => t.Height);

		foreach (var tile in innerSelection)
		{
			tile.UpdateHeight(height);
		}

		if (outerRadius <= innerRadius)
			return;
		var outerSelection = CircularSelect(center, outerRadius).Select(t => t as Tile3D).Except(innerSelection);
		foreach (var tile in outerSelection)
		{
			var d = Mathf.Pow(center.WorldX - tile.Coords.WorldX, 2) + Mathf.Pow(center.WorldZ - tile.Coords.WorldZ, 2);
			d -= innerRadius * innerRadius;
			d = MathUtils.Map(d, 0, (outerRadius * outerRadius) - (innerRadius * innerRadius), 0, 1);
			tile.UpdateHeight(Mathf.Lerp(tile.Height, height, 1-d));
		}
	}

	public void ReplaceTile(HexCoords tilePos, T newTile)
	{
		this[tilePos].DestroyTile();
		this[tilePos] = newTile;
		newTile.RenderTile(Parent);
	}

	public T[] GetNeighbors(T tile) => GetNeighbors(tile.Coords);

	public void Destroy()
	{
		foreach (var tile in Tiles)
			tile?.DestroyTile();
	}

	public IEnumerator<T> GetEnumerator()
	{
		return ((IEnumerable<T>)Tiles).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<T>)Tiles).GetEnumerator();
	}
}
