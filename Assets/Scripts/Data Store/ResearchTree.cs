using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchTree
{
	public ResearchTech baseNode;

	public ResearchTree(ResearchTech root)
	{
		baseNode = root;
	}

	public int Count()
	{
		return baseNode.DeepCount() + 1;
	}
}

public class ResearchTech : IEnumerable<ResearchTech>
{
	public static int CUR_ID;
	public int id;
	public string name;
	public string description;
	public bool isResearched;
	public ResourceIndentifier[] resourceCost;

	public ResearchTech[] children;
	public int Count { get; private set; }


	const int MAX_CHILDREN = 6;


	public ResearchTech(string name, string description = "", bool isResearched = false)
	{
		this.name = name;
		id = CUR_ID++;
		this.isResearched = isResearched;
		children = new ResearchTech[MAX_CHILDREN];
	}

	public ResearchTech AddChild(ResearchTech tech)
	{
		if (Count >= MAX_CHILDREN)
			throw new System.Exception("Tech is full");
		children[Count++] = tech;
		return this;
	}

	public IEnumerator<ResearchTech> GetEnumerator()
	{
		return ((IEnumerable<ResearchTech>)children).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<ResearchTech>)children).GetEnumerator();
	}

	public int DeepCount()
	{
		if (Count == 0)
			return Count;
		var count = Count;
		for (int i = 0; i < Count; i++)
		{
			count += children[i].DeepCount();
		}
		return count;
	}
}
