using Cysharp.Threading.Tasks;
using Snowship.NLife;
using Snowship.NResource;
using Snowship.NResource.NInventory;
using Snowship.NUI.UITab;
using TMPro;
using UnityEngine;
using static Snowship.NUtilities.ColourUtilities;

namespace Snowship.NUI
{
	public class UIHumanInfoPanelView : UIView {

		[Header("General Information")]
		[SerializeField] private UIHumanBodyElementComponent humanImage;
		[SerializeField] private TMP_Text nameText;
		[SerializeField] private TMP_Text currentActionText;
		[SerializeField] private TMP_Text storedActionText;
		[SerializeField] private TMP_Text affiliationText;

		[Header("Sliders")]
		[SerializeField] private UISliderHandler healthSlider;
		[SerializeField] private UISliderHandler moodSlider;
		[SerializeField] private UISliderHandler inventoryWeightSlider;
		[SerializeField] private TMP_Text inventoryWeightText;
		[SerializeField] private UISliderHandler inventoryVolumeSlider;
		[SerializeField] private TMP_Text inventoryVolumeText;

		[Header("Tabs")]
		[SerializeField] private Transform tabParent;
		[SerializeField] private Transform tabButtonParent;

		public async UniTask CreateTab(IUITabElement tab, UITabButton button, string buttonText) {
			await tab.OpenAsync(tabParent);
			tab.SetActive(false);
			await button.OpenAsync(tabButtonParent);
			button.SetText(buttonText);
		}

		public void SetGeneralInformation(Sprite skinSprite, string humanName, string affiliation) {
			humanImage.SetBodySectionSprite(EBodySection.Skin, skinSprite);
			nameText.SetText(humanName);
			affiliationText.SetText(affiliation);
		}

		public void SetCurrentActionText(string text) {
			currentActionText.SetText(text);
		}

		public void SetStoredActionText(string text) {
			storedActionText.SetText(text);
		}

		public void SetupHealthSlider((float min, float max) sliderRange, float healthValue, bool wholeNumbers) {
			healthSlider.SetFillColours(EColour.DarkRed, EColour.DarkGreen);
			healthSlider.SetHandleColours(EColour.LightRed, EColour.LightGreen);
			healthSlider.SetSliderRange(sliderRange.min, sliderRange.max, healthValue, wholeNumbers);
		}

		public void OnHealthChanged(float healthValue) {
			healthSlider.SetValue(healthValue);
		}

		public void SetupInventorySliders(Inventory inventory) {
			inventoryWeightSlider.SetSliderRange(0, inventory.maxWeight, inventory.UsedWeight());
			inventoryVolumeSlider.SetSliderRange(0, inventory.maxVolume, inventory.UsedVolume());
			OnInventoryChanged(inventory);
		}

		public void OnInventoryChanged(Inventory inventory) {
			int usedWeight = inventory.UsedWeight();
			int usedVolume = inventory.UsedVolume();
			inventoryWeightSlider.SetValue(usedWeight, $"{Mathf.RoundToInt(usedWeight / (float)inventory.maxWeight * 100)}");
			inventoryVolumeSlider.SetValue(usedVolume, $"{Mathf.RoundToInt(usedVolume / (float)inventory.maxVolume * 100)}");
		}

		public void SetupMoodSlider((float min, float max) sliderRange, float moodValue, bool wholeNumbers) {
			moodSlider.SetFillColours(EColour.DarkRed, EColour.DarkGreen);
			moodSlider.SetHandleColours(EColour.LightRed, EColour.LightGreen);
			moodSlider.SetSliderRange(sliderRange.min, sliderRange.max, moodValue, wholeNumbers);
		}

		public void OnMoodChanged(float effectiveMood, float moodModifiersSum) {
			moodSlider.SetValue(effectiveMood);
		}

		public void OnHumanClothingChanged(EBodySection bodySection, Clothing clothing) {
			humanImage.SetClothingOnBodySection(bodySection, clothing);
		}
	}
}
