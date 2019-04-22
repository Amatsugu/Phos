using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Generator/Feature/Resource")]
public class ResourceGenerator : FeatureGenerator
{
	public ResourceTileInfo resource;
	public int density;
	public float rarity;

	public override void Generate(Map map)
	{
		//TODO: Write generator for resources
	}
}
