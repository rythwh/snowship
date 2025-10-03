using System.Collections.Generic;
using Snowship.NLife;
using Snowship.NMap.NTile;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NHuman
{
	public class HumanView : LifeView<Human>
	{
		private ResourceManager ResourceM => GameManager.Get<ResourceManager>();

		public override void Bind(Human model) {
			base.Bind(model);

			Model.OnClothingChanged += OnClothingChanged;
		}

		protected override void OnAfterBind() {
			base.OnAfterBind();

			SetAppearance(Model.Clothes);
		}

		public override void Unbind() {
			base.Unbind();

			if (Model == null) {
				return;
			}

			Model.OnClothingChanged -= OnClothingChanged;
		}

		protected override void OnModelTileChanged(Tile tile) {
			base.OnModelTileChanged(tile);

			SetColour(tile.sr.color);
		}

		private void SetAppearance(Dictionary<BodySection, Clothing> clothes) {
			int appearanceIndex = 1;
			foreach (BodySection appearance in clothes.Keys) {
				transform.Find($"{BodySpriteRenderer.gameObject.name}/{appearance.ToString()}").GetComponent<SpriteRenderer>().sortingOrder = BodySpriteRenderer.sortingOrder + appearanceIndex;
				appearanceIndex += 1;
			}
		}

		private void OnClothingChanged(BodySection bodySection, Clothing clothing) {
			transform
				.Find(bodySection.ToString())
				.GetComponent<SpriteRenderer>()
				.sprite = clothing.image == null ? ResourceM.clearSquareSprite : clothing.image;
		}

		protected override void OnModelPositionChanged(Vector2 position) {
			base.OnModelPositionChanged(position);

			foreach ((BodySection bodySection, Clothing clothing) in Model.Clothes) {
				if (clothing != null) {
					transform.Find(bodySection.ToString()).GetComponent<SpriteRenderer>().sprite = clothing.moveSprites[MoveSpriteIndex];
				}
			}
		}
	}
}
