using System;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using Snowship.NLife;
using Snowship.NResource;

namespace Snowship.NUI
{
	public class UIClothingButtonElement : UIElement<UIClothingButtonElementComponent>
	{
		public readonly EBodySection BodySection;
		private Clothing clothing;

		public event Action<EBodySection> OnButtonClicked;

		public UIClothingButtonElement(EBodySection bodySection, Clothing clothing) {
			BodySection = bodySection;
			this.clothing = clothing;
		}

		protected override UniTask OnCreate() {
			Component.SetTypeText(BodySection.ToString());
			SetClothing(clothing);

			Component.OnButtonClicked += OnComponentButtonClicked;

			return UniTask.CompletedTask;
		}

		protected override void OnClose() {
			base.OnClose();

			Component.OnButtonClicked -= OnComponentButtonClicked;
			OnButtonClicked = null;
		}

		private void OnComponentButtonClicked() {
			OnButtonClicked?.Invoke(BodySection);
		}

		[SuppressMessage("ReSharper", "ParameterHidesMember")]
		public void SetClothing(Clothing clothing) {
			this.clothing = clothing;
			if (clothing != null) {
				Component.SetNameText(clothing.name);
				Component.SetImage(clothing.image);
			} else {
				Component.SetNameText("None");
				Component.SetImage(null);
			}
		}
	}
}
