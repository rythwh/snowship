using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.UITab
{
	public class UITabButtonComponent : UIElementComponent
	{
		[SerializeField] private Button button;
		[SerializeField] private TMP_Text buttonText;
		[SerializeField] private GameObject connector;

		public event Action ButtonClicked;

		public override void OnCreate() {
			base.OnCreate();

			button.onClick.AddListener(OnButtonClicked);
		}

		protected override void OnClose() {
			base.OnClose();

			button.onClick.RemoveListener(OnButtonClicked);
		}

		private void OnButtonClicked() {
			ButtonClicked?.Invoke();
		}

		public void SetText(string text) {
			buttonText.SetText(text);
		}

		public void ShowConnector(bool show) {
			connector.SetActive(show);
		}
	}
}
