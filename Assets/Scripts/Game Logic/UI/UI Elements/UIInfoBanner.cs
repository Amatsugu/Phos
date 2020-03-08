using TMPro;

using UnityEngine;

public class UIInfoBanner : MonoBehaviour
{
	private TMP_Text _text;

	private void Awake()
	{
		_text = GetComponentInChildren<TMP_Text>();
	}

	public void SetText(string text)
	{
		_text.SetText(text);
	}

	public void SetActive(bool active) => gameObject.SetActive(active);
}