using System;
using Cysharp.Threading.Tasks;
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

		public override UniTask OnCreate() {
			button.onClick.AddListener(OnButtonClicked);
			return UniTask.CompletedTask;
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
