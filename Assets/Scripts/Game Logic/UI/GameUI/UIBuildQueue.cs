using Amatsugu.Phos.UI;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amatsugu.Phos
{
    public class UIBuildQueue : UIPanel
    {

		private List<UIQueueItem> _items;
		private Dictionary<int, UIQueueItem> _activeItems;

		protected override void Awake()
		{
			base.Awake();
			_activeItems = new Dictionary<int, UIQueueItem>();
			_items = new List<UIQueueItem>();
			GameEvents.OnUnitQueued += OnUnitQueued;
			GameEvents.OnUnitConstructionStart += OnConstructionStart;
		}

		private void OnConstructionStart(ConstructionOrder order)
		{
			_activeItems[order.id].SetAsBuilding(order.buildCompleteTime, order.buildTime);
		}

		private void OnUnitQueued(BuildOrder order)
		{

			var curItem = _items[0];
			_items.RemoveAt(0);
			_activeItems.Add(order.id, curItem);
			curItem.Init(order);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			GameEvents.OnUnitQueued -= OnUnitQueued;
			GameEvents.OnUnitConstructionStart -= OnConstructionStart;
		}
	}
}
