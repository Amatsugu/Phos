using Amatsugu.Phos.Tiles;
using Amatsugu.Phos.UI;

using System.Collections.Generic;
using System.Linq;

using Unity.Entities;

using UnityEngine;

namespace Amatsugu.Phos
{
	public class UIBuildQueuePanel : UIPanel
	{
		public RectTransform content;
		public UIQueueItem queueItemPrefab;

		private List<UIQueueItem> _items;
		private Dictionary<int, UIQueueItem> _activeItems;
		private UnitFactorySystem _unitFactorySystem;
		private HexCoords _curFactory;

		protected override void Awake()
		{
			base.Awake();
			_activeItems = new Dictionary<int, UIQueueItem>();
			_items = new List<UIQueueItem>();
		}

		protected override void Start()
		{
			base.Start();
			_unitFactorySystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<UnitFactorySystem>();
			GameEvents.OnUnitQueued += OnUnitQueued;
			GameEvents.OnUnitConstructionStart += OnConstructionStart;
			GameEvents.OnUnitConstructionEnd += OnUnitRemoved;
			GameEvents.OnUnitDequeued += OnUnitRemoved;
		}


		public void Show(FactoryBuildingTile factory)
		{
			_curFactory = factory.Coords;
			var activeKeys = _activeItems.Keys.ToArray();
			for (int i = 0; i < activeKeys.Length; i++)
			{
				var k = _activeItems[activeKeys[i]];
				if (k.Building != factory.Coords || k.IsDone)
					k.SetActive(false);
				else
					k.SetActive(true);
			}
			Show();
		}

		private void OnConstructionStart(PendingUnitBuildOrder order)
		{
			if (!order.hasFactory || order.factoryCoords != _curFactory)
				return;
			_activeItems[order.id].SetAsBuilding(order.buildCompleteTime, order.buildTime);
		}

		private void OnUnitQueued(QueuedUnit order)
		{
			var curItem = GetQueueItem();
			_activeItems.Add(order.id, curItem);
			curItem.Init(order);
			curItem.OnClick += c =>
			{
				if (c.button == UnityEngine.EventSystems.PointerEventData.InputButton.Right)
					_unitFactorySystem.CancelOrder(order.id);
			};
		}

		private void OnUnitRemoved(int id)
		{
			if(_activeItems.ContainsKey(id))
			{
				var itm = _activeItems[id];
				itm.Finish();
				ReleaseQueueItem(itm);
			}
		}

		private UIQueueItem GetQueueItem()
		{
			if (_items.Count > 0)
			{
				var curItem = _items[0];
				_items.RemoveAt(0);
				return curItem;
			}else
			{
				var newItem = Instantiate(queueItemPrefab, content);
				return newItem;
			}
		}

		private void ReleaseQueueItem(UIQueueItem item)
		{
			item.ClearClickEvents();
			_items.Add(item);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			GameEvents.OnUnitQueued -= OnUnitQueued;
			GameEvents.OnUnitConstructionStart -= OnConstructionStart;
			GameEvents.OnUnitConstructionEnd -= OnUnitRemoved;
			GameEvents.OnUnitDequeued -= OnUnitRemoved;
		}
	}
}