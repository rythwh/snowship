using System;
using Cysharp.Threading.Tasks;
using Snowship.NResource;

namespace Snowship.NUI
{
	public class UIResourceAmountElement : UIElement<UIResourceAmountElementComponent>
	{
		public readonly ResourceAmount ResourceAmount;
		public event Action<UIResourceAmountElement> OnAmountChanged;

		public UIResourceAmountElement(ResourceAmount resourceAmount) {
			ResourceAmount = resourceAmount;
		}

		protected override UniTask OnCreate() {
			Component.SetResourceImage(ResourceAmount.Resource.image);
			Component.SetResourceName(ResourceAmount.Resource.name);

			ResourceAmount.OnAmountChanged += OnResourceAmountChanged;
			OnResourceAmountChanged(ResourceAmount.Amount);

			return UniTask.CompletedTask;
		}

		protected override void OnClose() {
			base.OnClose();
			ResourceAmount.OnAmountChanged -= OnResourceAmountChanged;
		}

		private void OnResourceAmountChanged(int amount) {
			Component.SetResourceAmount(amount.ToString());
			OnAmountChanged?.Invoke(this);
		}
	}
}
