using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Research Tree")]
public class ResearchTreeInfo : ScriptableObject
{
	public ResearchTree tree;

	private void OnEnable()
	{
		if (tree == null)
			tree = new ResearchTree(name);
	}
}