using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISelectionPanel : UIPanel
{
	public RectTransform selectionIconPrefab;

	private Dictionary<string, int> _selectionGroups;
	private List<List<ICommandable>> _selectionItems;
	private List<string> _selectionGroupNames;

	protected override void Awake()
	{
		base.Awake();
		_selectionGroups = new Dictionary<string, int>();
		_selectionItems = new List<List<ICommandable>>();
		_selectionGroupNames = new List<string>();
	}

	public void Show(ICommandable[] units)
	{
		_selectionGroups.Clear();
		_selectionItems.Clear();
		for (int i = 0; i < units.Length; i++)
		{
			var name = units[i].GetName();
			if (_selectionGroups.ContainsKey(name))
				_selectionItems[_selectionGroups[name]].Add(units[i]);
			else
			{
				_selectionGroups.Add(name, _selectionItems.Count);
				_selectionItems.Add(new List<ICommandable> { units[i] });
				_selectionGroupNames.Add(name);
			}
		}

		Show();
	}

	private void RenderUnits()
	{
		
	}

	private void RenderTiles()
	{

	}
}
