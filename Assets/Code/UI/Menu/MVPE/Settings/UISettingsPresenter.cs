using System.Linq;
using JetBrains.Annotations;
using Snowship.NPersistence;
using Snowship.NSettings;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{

	[UsedImplicitly]
	public class UISettingsPresenter : UIPresenter<UISettingsView> {
		private readonly SettingsState settings = GameManager.Get<SettingsManager>().SettingsState;
		private readonly SettingsState newSettings = new SettingsState();

		public UISettingsPresenter(UISettingsView view) : base(view) {
		}

		public override void OnCreate() {
			View.OnCancelButtonClicked += OnCancelButtonClicked;
			View.OnApplyButtonClicked += OnApplyButtonClicked;
			View.OnAcceptButtonClicked += OnAcceptButtonClicked;

			View.OnResolutionSliderChanged += OnResolutionSliderChanged;
			View.OnFullscreenToggled += OnFullscreenToggled;
			View.OnUIScaleToggled += OnUIScaleToggled;

			SetResolutionSlider();
			SetFullscreenToggle();
			SetUIScaleToggle();

			Debug.Log(settings.ToString());
		}

		public override void OnClose() {
			View.OnCancelButtonClicked -= OnCancelButtonClicked;
			View.OnApplyButtonClicked -= OnApplyButtonClicked;
			View.OnAcceptButtonClicked -= OnAcceptButtonClicked;

			View.OnResolutionSliderChanged -= OnResolutionSliderChanged;
			View.OnFullscreenToggled -= OnFullscreenToggled;
			View.OnUIScaleToggled -= OnUIScaleToggled;
		}

		private void OnCancelButtonClicked() {
			GameManager.Get<UIManager>().GoBack(this);
		}

		private void OnApplyButtonClicked() {
			settings.ApplySettings();
		}

		private void OnAcceptButtonClicked() {
			settings.ApplySettings();
			GameManager.Get<UIManager>().GoBack(this);
		}

		private void SetResolutionSlider() {
			View.SetResolutionSlider(
				0,
				Screen.resolutions.Length - 1,
				Screen.resolutions.ToList().IndexOf(settings.Resolution)
			);
		}

		private void SetFullscreenToggle() {
			View.SetFullscreenToggle(settings.Fullscreen);
		}

		private void SetUIScaleToggle() {
			View.SetUIScaleToggle(settings.ScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize);
		}

		private void OnResolutionSliderChanged(float sliderValue) {
			Resolution resolution = Screen.resolutions[Mathf.FloorToInt(sliderValue)];
			newSettings.SetResolution(resolution);
			View.SetResolutionText(resolution.ToString());
			UpdateButtonsForChanges();
		}

		private void OnFullscreenToggled(bool isOn) {
			newSettings.SetFullscreen(isOn);
			UpdateButtonsForChanges();
		}

		private void OnUIScaleToggled(bool isOn) {
			newSettings.SetScaleMode(isOn ? CanvasScaler.ScaleMode.ScaleWithScreenSize : CanvasScaler.ScaleMode.ConstantPixelSize);
			UpdateButtonsForChanges();
		}

		private void UpdateButtonsForChanges() {
			bool changesExist = !settings.Equals(newSettings);
			if (changesExist) {
				View.SetButtonColours(
					ColourUtilities.GetColour(ColourUtilities.EColour.LightRed),
					ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen),
					ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen)
				);
			} else {
				View.SetButtonColours(
					ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey200),
					ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey200),
					ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey200)
				);
			}
		}
	}
}
