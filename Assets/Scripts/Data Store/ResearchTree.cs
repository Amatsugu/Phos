using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResearchTree
{

	public ResearchTree(string name)
	{
		this.name = name;
		Reset();
	}

	public string name;
	public ResearchTech BaseNode => nodes[0];

	[SerializeField]
	public List<ResearchTech> nodes;

	public int Count;

	public ResearchTech AddChild(ResearchTech parent, ResearchTech child)
	{
		child.id = GetNextId();
		if (child.id >= nodes.Count)
			nodes.Add(child);
		else
			nodes[child.id] = child;
		Count++;

		nodes[parent.id].AddChild(child);
		return child;
	}

	public void Reset()
	{
		nodes = new List<ResearchTech>
		{
			new ResearchTech(name, isResearched: true)
		};
		Count = 1;
	}

	public void RemoveChild(ResearchTech parent, int childIndex)
	{
		var childId = parent.childrenIDs[childIndex];
		var child = GetChild(childId);
		var cCount = child.Count;
		if (cCount > 0)
		{
			for (int i = 0; i < cCount; i++)
			{
				RemoveChild(child, 0);
			}
		}
		parent.RemoveChild(childIndex);
		Count--;
		nodes[childId].id = -1;
	}

	public void MoveChild(int dstId, ResearchTech node)
	{
		var parent = GetParent(node);
		var nodeIndex = parent.IndexOf(node);
		parent.RemoveChild(nodeIndex);
		AddChild(nodes[dstId], node);
	}

	public bool IsDeepChild(ResearchTech parent, ResearchTech child)
	{
		if (parent.IsParentOf(child))
			return true;
		for (int i = 0; i < parent.Count; i++)
		{
			if(IsDeepChild(nodes[parent.childrenIDs[i]], child))
			{
				return true;
			}
		}
		return false;
	}

	public ResearchTech GetParent(ResearchTech child)
	{
		if (child.id == 0)
			return null;
		for (int i = 0; i < nodes.Count; i++)
		{
			if(nodes[i] != null)
			{
				if (nodes[i].IsParentOf(child))
					return nodes[i];
			}
		}
		return null;
	}

	public ResearchTech GetChild(int id)
	{
		return nodes[id];
	}

	public int GetNextId()
	{
		for (int i = 0; i < nodes.Count; i++)
		{
			if (!nodes[i].IsValid)
				return i;
		}
		return nodes.Count;
	}

	void OnEnable()
	{
		if(nodes == null)
		{
			Reset();
		}
	}


	public int GetDepth()
	{
		return BaseNode.GetDepth(this) + 1;
	}


	[System.Serializable]
	public class ResearchTech
	{
		public int id;
		public string name;
		public Sprite icon;
		public string description;
		public bool isResearched;
		public ResourceIndentifier[] resourceCost;
		public bool IsValid => id >= 0;

		//public ResearchTech[] children;
		public int[] childrenIDs;
		public int Count;


		public const int MAX_CHILDREN = 3;


		public ResearchTech(string name, string description = "", int id = 0, bool isResearched = false)
		{
			this.name = name;
			this.isResearched = isResearched;
			this.id = id;
			childrenIDs = new int[MAX_CHILDREN];
			resourceCost = new ResourceIndentifier[0];
			//children = new ResearchTech[MAX_CHILDREN];
		}

		internal ResearchTech AddChild(ResearchTech tech)
		{
			if (Count >= MAX_CHILDREN)
				throw new System.Exception("Tech Tree is full");
			childrenIDs[Count++] = tech.id;
			return this;
		}

		public int DeepCount(ResearchTree tree)
		{
			if (Count == 0)
				return Count;
			var count = Count;
			for (int i = 0; i < Count; i++)
			{
				count += tree.GetChild(childrenIDs[i]).DeepCount(tree);
			}
			return count;
		}

		public int GetDepth(ResearchTree tree, int curDepth = 0)
		{
			var d = curDepth;
			for (int i = 0; i < Count; i++)
			{
				var cD = tree.GetChild(childrenIDs[i]).GetDepth(tree, curDepth + 1);
				if (cD > d)
					d = cD;
			}
			return d;
		}

		internal void RemoveChild(int index)
		{
			if (index >= Count)
				throw new IndexOutOfRangeException($"{index} is greater than or equal to {Count}");
			for (int i = index + 1; i < childrenIDs.Length; i++)
			{
				childrenIDs[i - 1] = childrenIDs[i];
			}
			Count--;
		}

		public bool IsParentOf(ResearchTech child)
		{
			for (int i = 0; i < Count; i++)
			{
				if (childrenIDs[i] == child.id)
					return true;
			}
			return false;
		}

		public int IndexOf(ResearchTech child)
		{
			for (int i = 0; i < Count; i++)
			{
				if (childrenIDs[i] == child.id)
					return i;
			}
			return -1;
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is ResearchTech t)
				return t.id == id;
			else
				return false;
		}
	}

}


