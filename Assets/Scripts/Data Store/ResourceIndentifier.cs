using UnityEngine;

[System.Serializable]
public struct ResourceIndentifier
{
	public int id;
	public float ammount;

	public static ResourceIndentifier operator *(ResourceIndentifier r, float v) => new ResourceIndentifier { id = r.id, ammount = r.ammount * v };
}