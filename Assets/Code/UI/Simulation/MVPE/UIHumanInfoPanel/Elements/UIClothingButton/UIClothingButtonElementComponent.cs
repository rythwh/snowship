using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	public class UIClothingButtonElementComponent : UIElementComponent
	{
		[SerializeField] private Button button;
		[SerializeField] private TMP_Text typeText;
		[SerializeField] private TMP_Text nameText;
		[SerializeField] private Image image;

		public event Action OnButtonClicked;

		public override UniTask OnCreate() {
			button.onClick.AddListener(() => OnButtonClicked?.Invoke());
			return UniTask.CompletedTask;
		}

		protected override void OnClose() {
			button.onClick.RemoveListener(() => OnButtonClicked?.Invoke());
		}

		public void SetTypeText(string typeString) {
			typeText.SetText(typeString);
		}

		public void SetNameText(string nameString) {
			nameText.SetText(nameString);
		}

		public void SetImage(Sprite sprite) {
			bool spriteValid = sprite != null;

			if (spriteValid) {
				image.sprite = sprite;
			}

			image.gameObject.SetActive(spriteValid);
		}
	}
}
