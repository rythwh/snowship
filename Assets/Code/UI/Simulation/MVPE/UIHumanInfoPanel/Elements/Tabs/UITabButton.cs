using System;
using Cysharp.Threading.Tasks;

namespace Snowship.NUI.UITab
{
	public class UITabButton : UIElement<UITabButtonComponent>
	{
		public event Action ButtonClicked;

		protected override UniTask OnCreate() {
			Component.ButtonClicked += OnButtonClicked;
			return UniTask.CompletedTask;
		}

		private void OnButtonClicked() {
			ButtonClicked?.Invoke();
		}

		public void SetText(string text) {
			Component.SetText(text);
		}

		public void ShowConnector(bool show) {
			Component.ShowConnector(show);
		}
	}
}
