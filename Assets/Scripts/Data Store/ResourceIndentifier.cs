using UnityEngine;

[System.Serializable]
public struct ResourceIndentifier
{
	public int id;
	public float ammount;

	public ResourceIndentifier(int id, float ammount)
	{
		this.id = id;
		this.ammount = ammount;
	}

	public static ResourceIndentifier operator *(ResourceIndentifier r, float v) => new() { id = r.id, ammount = r.ammount * v };
}