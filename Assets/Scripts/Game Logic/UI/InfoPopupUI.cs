using Amatsugu.Phos.Tiles;

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

	private int AddPopup(Tile tile, Sprite icon, string title, string message)
	{
		UIInfoPopup popup;
		//Get new or pooled popup
		if (_availablePopups.Count == 0)
			popup = Instantiate(popupPrefab, _rectTransform);
		else
		{
			popup = _availablePopups.Last();
			_availablePopups.RemoveAt(_availablePopups.Count - 1);
		}
		//Init popup
		popup.Init(tile);
		_activePopups.Add(tile.Coords, popup);
		return popup.AddNotif(icon, title, message);
	}

	public void RemoveNotif(HexCoords coords, int notifId)
	{
		if (!_activePopups.ContainsKey(coords))
			return;
		if (_activePopups[coords].NotifCount == 1)
			RemovePopup(coords);
		else
			_activePopups[coords].RemoveNotif(notifId);
	}

	private void RemovePopup(HexCoords coords)
	{
		if (!_activePopups.ContainsKey(coords))
			return;
		_activePopups[coords].SetActive(false);
		_activePopups[coords].RemoveAll() ;
		_availablePopups.Add(_activePopups[coords]);
		_activePopups.Remove(coords);
	}

	public static int ShowPopupNotif(Tile tile, Sprite icon, string title, string message) => _INST.AddPopup(tile, icon, title, message);

	public static void HidePopup(HexCoords coords) => _INST.RemovePopup(coords);

	public static void RemovePopupNotif(HexCoords coords, int notifId) => _INST.RemoveNotif(coords, notifId);
}