using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
	public TMP_Text text;

	private TMP_Text[] _rText;

	void Start()
	{
		_rText = new TMP_Text[ResourceDatabase.ResourceCount];
		for (int i = 0; i < _rText.Length; i++)
		{
			_rText[i] = Instantiate(text, transform);
			_rText[i].rectTransform.anchoredPosition = new Vector2(0, _rText[i].rectTransform.rect.height * -i);
			_rText[i].text = $"{ResourceDatabase.GetResourceName(i)} : {HQSystem.resCount?[i]}";
		}
	}

	void Update()
	{
		for (int i = 0; i < _rText.Length; i++)
		{
			_rText[i].SetText($"<sprite={ResourceDatabase.GetSpriteId(i)}> {HQSystem.resCount?[i]}");
		}
	}
}
