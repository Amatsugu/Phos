using Amatsugu.Phos;
using Amatsugu.Phos.Tiles;

using Effects.Lines;

using System;

using TMPro;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.UI;

public class UIInfoPopup : UIExpandable
{
	[Header("Popup Settings")]
	public TMP_Text title;

	public TMP_Text desc;
	public float3 offset = new float3(0, 1, 0);
	public MeshEntityRotatable line;
	public float hoverSpacing;
	public float blurSpacing;

	public UIHover[] iconRects;
	public Image[] iconImages;

	public int NotifCount { get; private set; } = 0;

	private const int MAX_NOTIFS = 3;

	private Vector3 _notifPos;
	private Entity _line;
	private float _spacingTime;
	private string[] _titles;
	private string[] _messages;
	private int[] _ids;
	private int _nextId = 0;

#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		if (iconRects == null)
			iconRects = new UIHover[MAX_NOTIFS];
		else if (iconRects.Length != MAX_NOTIFS)
			Array.Resize(ref iconRects, MAX_NOTIFS);

		if (iconImages == null)
			iconImages = new Image[MAX_NOTIFS];
		else if (iconImages.Length != MAX_NOTIFS)
			Array.Resize(ref iconImages, MAX_NOTIFS);
	}
#endif

	protected override void Awake()
	{
		base.Awake();
		Clear();
		for (int i = 0; i < MAX_NOTIFS; i++)
		{
			var index = i;
			iconRects[i].SetActive(false);
			iconRects[i].OnHover += () =>
			{
				title.SetText(_titles[index]);
				desc.SetText(_messages[index]);
			};
		}
	}

	private void Clear()
	{
		_titles = new string[MAX_NOTIFS];
		_messages = new string[MAX_NOTIFS];
		_ids = new int[MAX_NOTIFS];
		_nextId = 0;
	}

	protected override void Start()
	{
		base.Start();
		Update();
	}

	public void Init(Tile tile)
	{
		SetActive(true);
		_notifPos = tile.SurfacePoint + offset;
		if (tile is BuildingTile b)
			_notifPos += new Vector3(0, b.buildingInfo.centerOfMassOffset.y, 0);
		rTransform.position = _notifPos;
		rTransform.rotation = GameRegistry.Camera.transform.rotation;
		Debug.DrawLine(tile.SurfacePoint, _notifPos, Color.magenta, 5);
		//_line = LineFactory.CreateStaticLine(line, _notifPos, tile.SurfacePoint);
	}

	public int AddNotif(Sprite icon, string title, string message)
	{
		var index = NotifCount++;
		iconRects[index].SetActive(true);
		_titles[index] = title;
		_messages[index] = message;
		var curId = _nextId;
		_ids[index] = _nextId++;
		return curId;
	}

	public void RemoveNotif(int id)
	{
		var idIndex = 0;
		for (int i = 0; i < MAX_NOTIFS; i++)
		{
			if (_ids[i] == id)
			{
				idIndex = _ids[i];
				break;
			}
		}
		for (int i = idIndex; i < MAX_NOTIFS; i++)
		{
			_titles[i] = i == MAX_NOTIFS - 1 ? null : _titles[i + 1];
			_messages[i] = i == MAX_NOTIFS - 1 ? null : _messages[i + 1];
			_ids[i] = i == MAX_NOTIFS - 1 ? -1 : _ids[i + 1];
		}
		iconRects[MAX_NOTIFS - 1].SetActive(false);
		iconRects[MAX_NOTIFS - 1].rTransform.anchoredPosition = Vector2.zero;
		NotifCount--;
	}

	public void RemoveAll()
	{
		Clear();
		NotifCount = 0;
	}

	protected override void Update()
	{
		base.Update();
		if (isHovered)
			_spacingTime += Time.deltaTime * 2;
		else
			_spacingTime -= Time.deltaTime * 2;
		_spacingTime = Mathf.Clamp(_spacingTime, 0, 1);
		for (int i = 0; i < NotifCount; i++)
		{
			var offset = i - ((NotifCount - 1) / 2f);
			var pos = new Vector2
			{
				x = (blurSpacing * offset).Lerp(offset * hoverSpacing, _spacingTime.EaseOut())
			};
			iconRects[i].rTransform.anchoredPosition = pos;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		try
		{
			//Map.EM.DestroyEntity(_line);
		}
		catch
		{
		}
	}
}