using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	public class UIClothingElementComponent : UIElementComponent
	{
		[SerializeField] private Button button;

		[SerializeField] private Image clothingImage;
		[SerializeField] private TMP_Text clothingNameText;

		[SerializeField] private TMP_Text waterResistanceValueText;
		[SerializeField] private TMP_Text insulationValueText;

		public event Action OnButtonClicked;

		public override UniTask OnCreate() {
			button.onClick.AddListener(() => OnButtonClicked?.Invoke());
			return UniTask.CompletedTask;
		}

		protected override void OnClose() {
			button.onClick.RemoveListener(() => OnButtonClicked?.Invoke());
		}

		public void SetClothingImage(Sprite clothingSprite) {
			clothingImage.sprite = clothingSprite;
		}

		public void SetClothingNameText(string clothingName) {
			clothingNameText.SetText(clothingName);
		}

		public void SetWaterResistanceValueText(string waterResistanceValue) {
			waterResistanceValueText.SetText(waterResistanceValue);
		}

		public void SetInsulationValueText(string insulationValue) {
			insulationValueText.SetText(insulationValue);
		}
	}
}
