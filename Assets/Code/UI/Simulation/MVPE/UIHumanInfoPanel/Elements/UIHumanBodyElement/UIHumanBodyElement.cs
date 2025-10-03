using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NHuman;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NUI
{
	[UsedImplicitly]
	public class UIHumanBodyElement : UIElement<UIHumanBodyElementComponent>
	{
		private readonly Human human;

		public UIHumanBodyElement(Human human) {
			this.human = human;

		}

		protected override UniTask OnCreate() {
			foreach ((BodySection appearance, Clothing clothing) in human.Clothes) {
				SetClothingOnBodySection(appearance, clothing);
			}
			return UniTask.CompletedTask;
		}

		public void SetClothingOnBodySection(BodySection bodySection, Clothing clothing) {
			Component.SetClothingOnBodySection(bodySection, clothing);
		}

		public void SetSpriteOnBodySection(Sprite sprite) {
			Component.SetBodySectionSprite(BodySection.Skin, sprite);
		}
	}
}
