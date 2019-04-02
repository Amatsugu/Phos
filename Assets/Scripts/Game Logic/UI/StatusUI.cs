using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class StatusUI : MonoBehaviour
{
	public TMP_Text text;
	public TMP_Text resourcePanel;

	private TMP_Text[] _rText;

	void Start()
	{

	}

	void Update()
	{
		var sb = new StringBuilder();
		for (int i = 0; i < ResourceDatabase.ResourceCount; i++)
		{
			sb.Append($"<sprite={ResourceDatabase.GetSpriteId(i)}> <size=.75em><voffset=.25em>{HQSystem.resCount?[i]}</voffset></size> ");
		}
		resourcePanel.SetText(sb);
	}
}
