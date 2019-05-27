using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Research Tree")]
public class ResearchTree : ScriptableObject
{
	public ResearchTech baseNode;

	void OnEnable()
	{
		if(baseNode == null)
			baseNode = new ResearchTech(name, isResearched: true);
	}

	public int Count()
	{
		return baseNode.DeepCount() + 1;
	}

	public int GetDepth()
	{
		return baseNode.GetDepth() + 1;
	}
}

public class ResearchTech : IEnumerable<ResearchTech>
{
	public static int CUR_ID;
	public int id;
	public string name;
	public Sprite icon;
	public string description;
	public bool isResearched;
	public ResourceIndentifier[] resourceCost;

	public ResearchTech[] children;
	public int Count { get; private set; }


	public const int MAX_CHILDREN = 3;


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

	public int GetDepth(int curDepth = 0)
	{
		var d = curDepth;
		for (int i = 0; i < Count; i++)
		{
			var cD = children[i].GetDepth(curDepth + 1);
			if (cD > d)
				d = cD;
		}
		return d;
	}

	public void RemoveChild(int index)
	{
		if (index >= Count)
			throw new IndexOutOfRangeException($"{index} is greater than or equal to {Count}");
		for (int i = index+1; i < children.Length; i++)
		{
			children[i - 1] = children[i];
			//if (i == Count)
				//children[i] = null;
		}
		Count--;
	}
}
