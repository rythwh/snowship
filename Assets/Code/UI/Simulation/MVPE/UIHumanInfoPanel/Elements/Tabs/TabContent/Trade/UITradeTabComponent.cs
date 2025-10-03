using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.UITab
{
	public class UITradeTabComponent : UITabElementComponent
	{
		[SerializeField] private Button tradeButton;

		public event Action TradeButtonClicked;

		public override UniTask OnCreate() {
			tradeButton.onClick.AddListener(OnTradeButtonClicked);
			return UniTask.CompletedTask;
		}

		public void OnTradeButtonClicked() {
			TradeButtonClicked?.Invoke();
		}
	}
}
