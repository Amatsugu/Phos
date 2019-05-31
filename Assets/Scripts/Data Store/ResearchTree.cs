using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Game Data/Research Tree")]
public class ResearchTree : ScriptableObject
{
	public ResearchTech BaseNode => _nodes[0];

	[SerializeField]
	private List<ResearchTech> _nodes;

	public int Count;

	public void AddChild(ResearchTech parent, ResearchTech child)
	{
		child.id = GetNextId();
		if (child.id >= _nodes.Count)
			_nodes.Add(child);
		else
			_nodes[child.id] = child;
		Debug.Log($"{parent.id} {child.id}");
		Count++;

		_nodes[parent.id].AddChild(child);
	}

	public void Reset()
	{
		_nodes = new List<ResearchTech>
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
		_nodes[childId].id = -1;
	}

	public ResearchTech GetChild(int id)
	{
		return _nodes[id];
	}

	public int GetNextId()
	{
		for (int i = 0; i < _nodes.Count; i++)
		{
			if (!_nodes[i].IsValid)
				return i;
		}
		return _nodes.Count;
	}

	void OnEnable()
	{
		if(_nodes == null)
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
		public int Count { get; private set; }


		public const int MAX_CHILDREN = 3;


		public ResearchTech(string name, string description = "", int id = 0, bool isResearched = false)
		{
			this.name = name;
			this.isResearched = isResearched;
			this.id = id;
			childrenIDs = new int[MAX_CHILDREN];
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


