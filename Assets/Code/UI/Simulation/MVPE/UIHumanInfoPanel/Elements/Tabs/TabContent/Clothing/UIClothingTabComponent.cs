using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Snowship.NLife;
using Snowship.NResource;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.UITab
{
	public class UIClothingTabComponent : UITabElementComponent
	{
		[Header("Current Clothing Sub-Panel")]
		[SerializeField] private UIHumanBodyElementComponent humanBody;
		[SerializeField] private VerticalLayoutGroup clothingButtonsListVerticalLayoutGroup;

		[Header("Clothing Selection Sub-Panel")]
		[SerializeField] private GameObject clothingSelectionPanel;
		[SerializeField] private Button disrobeButton;
		[SerializeField] private Button clothingSelectionPanelBackButton;
		[SerializeField] private GameObject clothesAvailableTitleTextPanel;
		[SerializeField] private GridLayoutGroup clothesAvailableGridLayoutGroup;
		[SerializeField] private GameObject clothesTakenTitleTextPanel;
		[SerializeField] private GridLayoutGroup clothesTakenGridLayoutGroup;

		private readonly List<UIClothingButtonElement> clothingButtonElements = new();
		private readonly List<UIClothingElement> clothingElements = new();

		public override UniTask OnCreate() {
			clothingSelectionPanelBackButton.onClick.AddListener(() => SetClothingSelectionPanelActive(false));
			return UniTask.CompletedTask;
		}

		protected override void OnClose() {
			base.OnClose();

			foreach (UIClothingButtonElement clothingButtonElement in clothingButtonElements) {
				clothingButtonElement.Close();
			}
			clothingButtonElements.Clear();

			foreach (UIClothingElement clothingElement in clothingElements) {
				clothingElement.Close();
			}
			clothingElements.Clear();
		}

		public void SetBodySectionSprite(Sprite skinSprite) {
			humanBody.SetBodySectionSprite(EBodySection.Skin, skinSprite);
		}

		public async UniTask<UIClothingButtonElement> CreateClothingButton(EBodySection bodySection, Clothing clothing) {
			UIClothingButtonElement clothingButton = new(bodySection, clothing);
			await clothingButton.OpenAsync(clothingButtonsListVerticalLayoutGroup.transform);
			clothingButtonElements.Add(clothingButton);
			return clothingButton;
		}

		public void SetClothingSelectionPanelActive(bool active) {

			foreach (UIClothingElement clothingElement in clothingElements) {
				clothingElement.Close();
			}
			clothingElements.Clear();

			clothingSelectionPanel.SetActive(active);
		}

		public void SetDisrobeButtonTarget(EBodySection bodySection) {
			disrobeButton.onClick.RemoveAllListeners();
			disrobeButton.onClick.AddListener(() => OnDisrobeButtonClicked(bodySection));
		}

		private void OnDisrobeButtonClicked(EBodySection bodySection) {
			OnHumanClothingChanged(bodySection, null);
			SetClothingSelectionPanelActive(false);
		}

		public async UniTask<UIClothingElement> CreateAvailableClothingElement(Clothing clothing) {
			return await CreateClothingElement(clothing, true);
		}

		public async UniTask<UIClothingElement> CreateTakenClothingElement(Clothing clothing) {
			return await CreateClothingElement(clothing, false);
		}

		private async UniTask<UIClothingElement> CreateClothingElement(Clothing clothing, bool available) {
			UIClothingElement clothingElement = new(clothing);
			await clothingElement.OpenAsync((available ? clothesAvailableGridLayoutGroup : clothesTakenGridLayoutGroup).transform);
			clothingElements.Add(clothingElement);
			return clothingElement;
		}

		public void SetClothesAvailableTitleTextPanelActive(bool active) {
			clothesAvailableTitleTextPanel.SetActive(active);
		}

		public void SetClothesTakenTitleTextPanelActive(bool active) {
			clothesTakenTitleTextPanel.SetActive(active);
		}

		public void OnHumanClothingChanged(EBodySection bodySection, Clothing clothing) {
			foreach (UIClothingButtonElement clothingButtonElement in clothingButtonElements) {
				if (clothingButtonElement.BodySection != bodySection) {
					continue;
				}
				clothingButtonElement.SetClothing(clothing);
				humanBody.SetClothingOnBodySection(bodySection, clothing);

				return;
			}
		}
	}
}
