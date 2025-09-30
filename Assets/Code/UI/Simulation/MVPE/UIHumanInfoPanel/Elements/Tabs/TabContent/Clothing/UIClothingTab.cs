using System.Collections.Generic;
using JetBrains.Annotations;
using Snowship.NHuman;
using Snowship.NResource;

namespace Snowship.NUI.UITab
{
	[UsedImplicitly]
	public class UIClothingTab : UITabElement<UIClothingTabComponent>
	{
		private readonly Human human;

		public UIClothingTab(Human human) {
			this.human = human;
		}

		protected override async void OnCreate() {
			base.OnCreate();

			foreach ((BodySection bodySection, Clothing clothing) in human.clothes) {
				UIClothingButtonElement clothingButton = await Component.CreateClothingButton(bodySection, clothing);
				clothingButton.OnButtonClicked += OnClothingButtonClicked;
			}

			human.OnClothingChanged += Component.OnHumanClothingChanged;
			Component.SetClothingSelectionPanelActive(false);
			Component.SetBodySectionSprite(human.moveSprites[0]);
		}

		protected override void OnClose() {
			base.OnClose();

			human.OnClothingChanged -= Component.OnHumanClothingChanged;
		}

		private async void OnClothingButtonClicked(BodySection bodySection) {

			Component.SetClothingSelectionPanelActive(true);
			Component.SetClothesAvailableTitleTextPanelActive(false);
			Component.SetClothesTakenTitleTextPanelActive(false);

			Component.SetDisrobeButtonTarget(bodySection);

			List<Clothing> clothesByAppearance = Clothing.GetClothesByAppearance(bodySection);
			foreach (Clothing clothing in clothesByAppearance) {
				if (clothing.GetWorldTotalAmount() <= 0) {
					continue;
				}

				UIClothingElement clothingElement = null;

				if (clothing.GetAvailableAmount() > 0) {
					clothingElement = await Component.CreateAvailableClothingElement(clothing);
					Component.SetClothesAvailableTitleTextPanelActive(true);
				} else if (human.clothes[bodySection] == null || clothing.name != human.clothes[bodySection].name) {
					clothingElement = await Component.CreateTakenClothingElement(clothing);
					Component.SetClothesTakenTitleTextPanelActive(true);
				}

				if (clothingElement != null) {
					clothingElement.OnButtonClicked += OnClothingElementClicked;
				}
			}
		}

		private void OnClothingElementClicked(Clothing clothing) {
			human.ChangeClothing(clothing.prefab.BodySection, clothing);
			Component.SetClothingSelectionPanelActive(false);
		}
	}
}
