using System;
using System.Collections.Generic;
using Snowship.NLife;
using Snowship.NResource;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	public class UIHumanBodyElementComponent : UIElementComponent
	{
		[SerializeField] private List<UIHumanBodyClothingSection> clothingSections = new();

		public void SetClothingOnBodySection(EBodySection bodySection, Clothing clothing) {
			SetBodySectionSprite(bodySection, clothing.moveSprites[0]);
		}

		public void SetBodySectionSprite(EBodySection bodySection, Sprite sprite) {
			foreach (UIHumanBodyClothingSection clothingSection in clothingSections) {
				if (clothingSection.bodySection != bodySection) {
					continue;
				}
				if (clothingSection.sectionImage == null) {
					continue;
				}

				clothingSection.sectionImage.sprite = sprite;
			}
		}
	}

	[Serializable]
	public struct UIHumanBodyClothingSection
	{
		public EBodySection bodySection;
		public Image sectionImage;
	}
}