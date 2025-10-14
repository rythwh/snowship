using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NLife;
using Snowship.NMap.NTile;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NHuman
{
	public class HumanView : LifeView<Human>
	{
		public override void Bind(
			Human model,
			Sprite[] moveSprites
		) {
			base.Bind(model, moveSprites);

			Model.OnClothingChanged += OnClothingChanged;
		}

		protected override void OnAfterBind() {
			base.OnAfterBind();

			SetInitialAppearance(Model.Clothes);
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

		private void SetInitialAppearance(Dictionary<EBodySection, Clothing> clothes) {
			foreach (BodySectionView bodySectionView in uBodySections) {
				bodySectionView.SetSortingOrder(uBodySpriteRenderer.sortingOrder);
				if (clothes.TryGetValue(bodySectionView.GetBodySection(), out Clothing clothing)) {
					bodySectionView.SetClothing(clothing);
				}
			}
		}

		private void OnClothingChanged(EBodySection bodySection, Clothing clothing) {
			BodySectionView bodySectionView = uBodySections.FirstOrDefault(bs => bs.GetBodySection() == bodySection);
			if (bodySectionView == null) {
				throw new ArgumentNullException($"Body section {bodySection.ToString()} does not exist on {Model.Name}");
			}
			bodySectionView.SetClothing(clothing);
		}
	}
}
