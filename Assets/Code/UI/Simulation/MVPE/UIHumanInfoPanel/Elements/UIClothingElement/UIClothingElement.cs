using System;
using Cysharp.Threading.Tasks;
using Snowship.NResource;

namespace Snowship.NUI
{
	public class UIClothingElement : UIElement<UIClothingElementComponent>
	{
		private readonly Clothing clothing;

		public event Action<Clothing> OnButtonClicked;

		public UIClothingElement(Clothing clothing) {
			this.clothing = clothing;
		}

		protected override UniTask OnCreate() {
			Component.SetClothingImage(clothing.image);
			Component.SetClothingNameText(clothing.name);
			Component.SetWaterResistanceValueText(clothing.prefab.waterResistance.ToString());
			Component.SetInsulationValueText(clothing.prefab.insulation.ToString());

			Component.OnButtonClicked += OnComponentButtonClicked;

			return UniTask.CompletedTask;
		}

		private void OnComponentButtonClicked() {
			OnButtonClicked?.Invoke(clothing);
		}
	}
}
