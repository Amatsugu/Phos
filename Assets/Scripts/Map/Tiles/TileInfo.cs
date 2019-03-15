using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/TileInfo")]
public class TileInfo : ScriptableObject
{
	public GameObject tilePrefab;
	public Material material;
	public Mesh mesh;
}
