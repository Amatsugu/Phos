using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class StatusUI : MonoBehaviour
{
	public TMP_Text resourcePanel;

	private StringBuilder _resourceText;

	void Start()
	{
		_resourceText = new StringBuilder();
	}

	void Update()
	{
		_resourceText.Clear();
		for (int i = 0; i < ResourceDatabase.ResourceCount; i++)
		{
			_resourceText.Append($"<sprite={ResourceDatabase.GetSpriteId(i)}>");
			var lastTickNet = ResourceSystem.totalProduction?[i] + ResourceSystem.totalDemand?[i];
			_resourceText.Append($"<size=.75em><voffset=.25em>{ResourceSystem.resCount?[i]} [{(lastTickNet >= 0 ? "+" : "")}{lastTickNet}]</voffset></size> ");
		}
		resourcePanel.SetText(_resourceText);
	}

}
