using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Amatsugu.Phos.UI
{
	public class UIQueueItem : UIButtonHover, IPointerClickHandler
	{
		public Image icon;
		public TMP_Text name;
		public event Action<PointerEventData> OnClick;


		private bool _isBuilding;
		private double _completionTime;
		private float _buildTime;

		public void Init(BuildOrder buildOrder)
		{
			var unit = GameRegistry.UnitDatabase[buildOrder.unit].info;

			name.SetText(unit.name);
			icon.sprite = unit.icon;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			OnClick?.Invoke(eventData);
		}

		public void ClearClickEvents()
		{
			OnClick = null;
		}

		public void SetAsBuilding(double completionTime, float buildTime)
		{
			_isBuilding = true;
			_completionTime = completionTime;
			_buildTime = buildTime;
		}

		protected override void Update()
		{
			base.Update();
			if (!_isBuilding)
				return;
			var remainingTime = _completionTime - Time.time;
			var prog = remainingTime / _buildTime;

		}
	}
}
