using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIResourceDisplay : MonoBehaviour
{
	public Image icon;
	public TMP_Text storedText;
	public TMP_Text incomeText;
	public TMP_Text consumeText;
	public TMP_Text netText;
	public TMP_Text netSignText;


	public Color negativeColor = Color.red;
	public Color positiveColor = Color.green;
	public Color warningColor = Color.yellow;
	public Color neutralColor = Color.white;

	private bool _atMax = false;
	private float _animProgress;
	private RectTransform _thisTransform;

    void Awake()
	{
		_thisTransform = GetComponent<RectTransform>();
		SetInfo(0, 0, 0, 0, false);
	}


	public void SetIcon(Sprite sprite)
	{
		icon.sprite = sprite;
	}

	public void SetWidth(float width)
	{
		_thisTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
	}

	public void SetInfo(int stored, int income, int consumption, int netIncome, bool atMax = false)
	{
		storedText.SetText(stored.ToString());
		incomeText.SetText(income.ToString());
		consumeText.SetText(consumption.ToString());
		netText.SetText(netIncome.ToString());
		if(netIncome == 0)
		{
			netText.color = warningColor;
			netSignText.color = warningColor;
			netSignText.SetText("+");
		}else if(netIncome > 0)
		{
			netText.color = positiveColor;
			netSignText.color = positiveColor;
			netSignText.SetText("+");
		}
		else if(netIncome < 0)
		{
			netText.color = negativeColor;
			netSignText.color = negativeColor;
			netSignText.SetText("-");
		}
		if(atMax != _atMax)
		{
			if(atMax)
			{
				StartCoroutine(FlashResourceCapWarning());
			}else
			{
				StopAllCoroutines();
				storedText.color = neutralColor;
			}
			_atMax = atMax;
		}
	}

	IEnumerator FlashResourceCapWarning()
	{
		if (_animProgress >= 1)
			_animProgress = 0;
		storedText.color = Color.Lerp(neutralColor, warningColor, Mathf.Round(_animProgress += Time.deltaTime));
		yield return new WaitForEndOfFrame();
	}

	
}
