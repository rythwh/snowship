using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NHuman;
using Snowship.NLife;
using Snowship.NResource;

namespace Snowship.NUI.UITab
{
	[UsedImplicitly]
	public class UIClothingTab : UITabElement<UIClothingTabComponent>
	{
		private readonly Human human;
		private readonly HumanView humanView;

		public UIClothingTab(Human human, HumanView humanView) {
			this.human = human;
			this.humanView = humanView;
		}

		protected override async UniTask OnCreate() {
			foreach ((EBodySection bodySection, Clothing clothing) in human.Clothes) {
				UIClothingButtonElement clothingButton = await Component.CreateClothingButton(bodySection, clothing);
				clothingButton.OnButtonClicked += OnClothingButtonClicked;
			}

			human.OnClothingChanged += Component.OnHumanClothingChanged;
			Component.SetClothingSelectionPanelActive(false);
			Component.SetBodySectionSprite(humanView.GetForwardFacingSprite());
		}

		protected override void OnClose() {
			base.OnClose();

			human.OnClothingChanged -= Component.OnHumanClothingChanged;
		}

		private async void OnClothingButtonClicked(EBodySection bodySection) {

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
				} else if (human.Clothes[bodySection] == null || clothing.name != human.Clothes[bodySection].name) {
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
