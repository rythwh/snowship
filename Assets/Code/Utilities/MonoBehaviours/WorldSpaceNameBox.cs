using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUtilities
{
	public class WorldSpaceNameBox : MonoBehaviour
	{
		[SerializeField] private Canvas Canvas;
		[SerializeField] private TMP_Text Text;

		[SerializeField] private Image NameColourImage;
		[SerializeField] private Transform NameBackgroundImage;
		[SerializeField] private Image HealthIndicatorImage;

		private readonly Color colourLowHealth = ColourUtilities.GetColour(ColourUtilities.EColour.LightRed100);
		private readonly Color colourHighHealth = ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen100);

		private void Awake() {
			Canvas.sortingOrder = (int)SortingOrder.WorldSpaceUI;
		}

		public void OnNameChanged(string name)
		{
			Text.SetText(name);
		}

		public void OnNameColourChanged(Color colour) {
			NameColourImage.color = colour;
		}

		public void OnCameraZoomChanged(float zoom) {
			const int min = 2;
			const int max = 10;

			Canvas.transform.localScale = Vector2.one * (Mathf.Clamp(zoom, min, max) * 0.001f);
		}

		public void OnHealthChanged(float health) {
			HealthIndicatorImage.color = Color.Lerp(
				colourLowHealth,
				colourHighHealth,
				health
			);
		}
	}
}
