using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Rarity Colors")]
public class RarityColors : ScriptableObject
{
	public Color[] Colors;

	public string ColorizeAsString(string name, int rarity)
	{
		return Colorize(name, rarity).ToString();
	}

	public StringBuilder Colorize(string name, int rarity)
	{
		var sb = new StringBuilder();
		sb.Append("<color=#");
		sb.Append(ColorUtility.ToHtmlStringRGB(Colors[rarity]));
		sb.Append('>');
		sb.Append(name);
		sb.Append("</color>");
		return sb;
	}
}
