using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonLink : MonoBehaviour
{
	public string url;

	private Button _btn;
	void Start()
	{
		_btn = GetComponent<Button>();
		_btn.onClick.AddListener(() =>
		{
			Application.OpenURL(url);
		});
	}
}
