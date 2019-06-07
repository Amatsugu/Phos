using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Research Tree")]
public class ResearchTreeInfo : ScriptableObject
{
	public ResearchTree tree;

	void OnEnable()
	{
		if (tree == null)
			tree = new ResearchTree(name);
	}
}
