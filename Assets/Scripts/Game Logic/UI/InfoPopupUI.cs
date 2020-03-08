using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class InfoPopupUI : MonoBehaviour
{
	public UIInfoPopup popupPrefab;

	private static InfoPopupUI _INST;

	private Dictionary<HexCoords, UIInfoPopup> _activePopups;

	private List<UIInfoPopup> _availablePopups;
	private HashSet<UIInfoPopup> _usedPopups;
	private RectTransform _rectTransform;

	private void Awake()
	{
		_INST = this;
		_rectTransform = GetComponent<RectTransform>();
	}

	private void Start()
	{
		_activePopups = new Dictionary<HexCoords, UIInfoPopup>();
		_availablePopups = new List<UIInfoPopup>();
	}

	// Update is called once per frame
	private void Update()
	{
	}

	private void AddPopup(HexCoords coords, Sprite icon, string title, string message)
	{
		UIInfoPopup popup;
		if (_availablePopups.Count == 0)
			popup = Instantiate(popupPrefab, _rectTransform);
		else
		{
			popup = _availablePopups.Last();
			_availablePopups.RemoveAt(_availablePopups.Count - 1);
		}
		popup.Init(coords);
		popup.AddNotif(icon, title, message);
		_activePopups.Add(coords, popup);
	}

	private void RemovePopup(HexCoords coords)
	{
		if (!_activePopups.ContainsKey(coords))
			return;
		_activePopups[coords].SetActive(false);
		_availablePopups.Add(_activePopups[coords]);
		_activePopups.Remove(coords);
	}

	public static void ShowPopup(HexCoords coords, Sprite icon, string title, string message) => _INST.AddPopup(coords, icon, title, message);

	public static void HidePopup(HexCoords coords) => _INST.RemovePopup(coords);
}