using System;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.UITab
{
	public class UITradeTabComponent : UITabElementComponent
	{
		[SerializeField] private Button tradeButton;

		public event Action TradeButtonClicked;

		public override void OnCreate() {
			base.OnCreate();

			tradeButton.onClick.AddListener(OnTradeButtonClicked);
		}

		public void OnTradeButtonClicked() {
			TradeButtonClicked?.Invoke();
		}
	}
}
