using Amatsugu.Phos.Tiles;

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
		public HexCoords Building { get; private set; }
		public bool IsDone { get; private set; }

		public Image icon;
		public Image mask;
		public TMP_Text nameText;
		public event Action<PointerEventData> OnClick;


		private bool _isBuilding;
		private float _completionTime;
		private float _buildTime;


		public void Init(QueuedUnit buildOrder)
		{
			var unit = GameRegistry.UnitDatabase[buildOrder.unit].info;
			if (nameText != null)
				nameText.SetText(unit.name);
			icon.sprite = unit.icon;
			rTransform.SetAsLastSibling();
			gameObject.SetActive(true);
			mask.fillAmount = 1;
			Building = buildOrder.factory;
			IsDone = false;
			_isBuilding = false;
		}

		public void Finish()
		{
			IsDone = true;
			ClearClickEvents();
			SetActive(false);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			OnClick?.Invoke(eventData);
		}

		public void ClearClickEvents()
		{
			OnClick = null;
		}

		public override void ClearAllEvents()
		{
			base.ClearAllEvents();
			ClearClickEvents();
		}

		public void SetAsBuilding(double completionTime, double buildTime)
		{
			_isBuilding = true;
			_completionTime = (float)completionTime;
			_buildTime = (float)buildTime;
			mask.fillAmount = 1;
		}

		protected override void Update()
		{
			base.Update();
			if (!_isBuilding || IsDone)
				return;
			var remainingTime = _completionTime - Time.time;
			var prog = remainingTime / _buildTime;
			mask.fillAmount = prog;
		}
	}
}
