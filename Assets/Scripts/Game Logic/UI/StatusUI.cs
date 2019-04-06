using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
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
            sb.Append($"<sprite={ResourceDatabase.GetSpriteId(i)}>");
            sb.Append($"<size=.75em><voffset=.25em>{ResourceSystem.resCount?[i]} [{(ResourceSystem.lastTickNet?[i] >= 0 ? "+" : "")}{ResourceSystem.lastTickNet?[i]}]</voffset></size> ");
		}
		resourcePanel.SetText(sb);
	}

}
