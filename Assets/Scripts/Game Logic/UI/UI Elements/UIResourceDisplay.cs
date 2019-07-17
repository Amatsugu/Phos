using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIResourceDisplay : UIHover
{
	public Image icon;
	public TMP_Text storedText;
	public TMP_Text incomeText;
	public TMP_Text consumeText;
	public TMP_Text netText;
	public TMP_Text netSignText;
	public RectTransform logisticsRect;
	public RectTransform inOutRect;
	public float expandSpeed;


	public Color negativeColor = Color.red;
	public Color positiveColor = Color.green;
	public Color warningColor = Color.yellow;
	public Color neutralColor = Color.white;

	private bool _atMax = false;
	private float _animProgress;
	private RectTransform _thisTransform;
	private bool _expanded = true;
	private float _expandProgress;
	private float _baseWidth;
	private float _inOutWidth;
	private float _logisticsWidth;

    protected override void Awake()
	{
		_thisTransform = GetComponent<RectTransform>();
		SetInfo(0, 0, 0, 0, false);

		_inOutWidth = inOutRect.rect.width;
		_logisticsWidth = logisticsRect.rect.width;
		_baseWidth = _thisTransform.rect.width;
		Close();

		OnHover += Open;
		OnBlur += Close;
	}


	public void Close()
	{
		if (!_expanded)
			return;
		_expanded = false;
	}


	protected override void Update()
	{
		if(_expanded)
		{
			_expandProgress += Time.deltaTime * expandSpeed;
		}else
			_expandProgress -= Time.deltaTime * expandSpeed;
		_expandProgress = Mathf.Clamp(_expandProgress, 0, 1);
		var t = _expandProgress * _expandProgress * _expandProgress;
		_thisTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(_baseWidth - _inOutWidth, _baseWidth, t));
		logisticsRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(_logisticsWidth - _inOutWidth, _logisticsWidth, t));

	}

	public void Open()
	{
		if (_expanded)
			return;
		_expanded = true;
	}

	public void SetIcon(Sprite sprite)
	{
		icon.sprite = sprite;
	}

	public void SetInfo(int stored, int income, int consumption, int netIncome, bool atMax = false)
	{
		storedText.SetText(stored.ToString());
		incomeText.SetText(income.ToString());
		consumeText.SetText(consumption.ToString());
		netText.SetText(Mathf.Abs(netIncome).ToString());
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
