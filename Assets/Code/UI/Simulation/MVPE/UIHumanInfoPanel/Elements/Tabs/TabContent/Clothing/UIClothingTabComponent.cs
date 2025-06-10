using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Snowship.NHuman;
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

		public override void OnCreate() {
			base.OnCreate();

			clothingSelectionPanelBackButton.onClick.AddListener(() => SetClothingSelectionPanelActive(false));
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
			humanBody.SetBodySectionSprite(BodySection.Skin, skinSprite);
		}

		public UIClothingButtonElement CreateClothingButton(BodySection bodySection, Clothing clothing) {
			UIClothingButtonElement clothingButton = new(bodySection, clothing);
			clothingButton.Open(clothingButtonsListVerticalLayoutGroup.transform).Forget();
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

		public void SetDisrobeButtonTarget(BodySection bodySection) {
			disrobeButton.onClick.RemoveAllListeners();
			disrobeButton.onClick.AddListener(() => OnDisrobeButtonClicked(bodySection));
		}

		private void OnDisrobeButtonClicked(BodySection bodySection) {
			OnHumanClothingChanged(bodySection, null);
			SetClothingSelectionPanelActive(false);
		}

		public UIClothingElement CreateAvailableClothingElement(Clothing clothing) {
			return CreateClothingElement(clothing, true);
		}

		public UIClothingElement CreateTakenClothingElement(Clothing clothing) {
			return CreateClothingElement(clothing, false);
		}

		private UIClothingElement CreateClothingElement(Clothing clothing, bool available) {
			UIClothingElement clothingElement = new(clothing);
			clothingElement.Open((available ? clothesAvailableGridLayoutGroup : clothesTakenGridLayoutGroup).transform).Forget();
			clothingElements.Add(clothingElement);
			return clothingElement;
		}

		public void SetClothesAvailableTitleTextPanelActive(bool active) {
			clothesAvailableTitleTextPanel.SetActive(active);
		}

		public void SetClothesTakenTitleTextPanelActive(bool active) {
			clothesTakenTitleTextPanel.SetActive(active);
		}

		public void OnHumanClothingChanged(BodySection bodySection, Clothing clothing) {
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
