using Snowship.NResource;
using UnityEngine;

namespace Snowship.NLife
{
	public class BodySectionView : MonoBehaviour
	{
		[SerializeField] private EBodySection uBodySection;
		private SpriteRenderer spriteRenderer;

		private int sortingOrderOffset = 0;

		private Color baseColour = Color.white;
		private Color ambientColour = Color.white;

		private Clothing clothing;

		public void Initialize()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
			sortingOrderOffset = transform.GetSiblingIndex() + 1;
		}

		public EBodySection GetBodySection()
		{
			return uBodySection;
		}

		private void SetBaseColour(Color newBaseColour)
		{
			baseColour = newBaseColour;
			ApplyColours();
		}

		public void SetAmbientColour(Color newAmbientColour)
		{
			ambientColour = newAmbientColour;
			ApplyColours();
		}

		private void ApplyColours()
		{
			spriteRenderer.color = baseColour * ambientColour;
		}

		public void SetSortingOrder(int baseSortingOrder)
		{
			spriteRenderer.sortingOrder = baseSortingOrder + sortingOrderOffset;
		}

		public void SetClothing(Clothing newClothing)
		{
			if (newClothing != null) {
				clothing = newClothing;
				spriteRenderer.sprite = clothing.image;
				SetBaseColour(Color.white);
				gameObject.SetActive(true);
			} else {
				clothing = null;
				spriteRenderer.sprite = null;
				SetBaseColour(Color.clear);
				gameObject.SetActive(false);
			}
		}

		public void SetMoveSprite(int moveSpriteIndex)
		{
			if (clothing == null) {
				return;
			}

			spriteRenderer.sprite = clothing.moveSprites[moveSpriteIndex];
		}
	}
}
