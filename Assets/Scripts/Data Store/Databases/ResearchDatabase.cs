using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Research Database")]
public class ResearchDatabase : ScriptableObject, ISerializationCallbackReceiver
{
	public Dictionary<BuildingCategory, ResearchTreeInfo> trees;

	[SerializeField]
	public ResearchTreeInfo[] treeValues;

	[SerializeField]
	public BuildingCategory[] treeKeys;

	public void OnBeforeSerialize()
	{
		treeValues = trees.Values.ToArray();
		treeKeys = trees.Keys.ToArray();
	}

	public void OnAfterDeserialize()
	{
		trees = new Dictionary<BuildingCategory, ResearchTreeInfo>();
		for (int i = 0; i < treeValues.Length; i++)
		{
			trees.Add(treeKeys[i], treeValues[i]);
		}
	}

	public ResearchTree this[BuildingCategory category] => trees[category].tree;

	public ResearchTree.ResearchTech this[ResearchIdentifier identifier] => trees[identifier.category].tree.GetChild(identifier.researchId);
}