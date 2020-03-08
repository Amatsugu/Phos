using System;

using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
	AttributeTargets.Class, Inherited = true)]
public class CreateNewAssetAttribute : PropertyAttribute
{
	public System.Type type;
	public string path;

	public CreateNewAssetAttribute(string path, System.Type type)
	{
		this.type = type;
		this.path = path;
	}
}