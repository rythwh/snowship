using System;

namespace Snowship.NUI.UITab
{
	public class UITabButton : UIElement<UITabButtonComponent>
	{
		public event Action ButtonClicked;

		protected override void OnCreate() {
			base.OnCreate();

			Component.ButtonClicked += OnButtonClicked;
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
