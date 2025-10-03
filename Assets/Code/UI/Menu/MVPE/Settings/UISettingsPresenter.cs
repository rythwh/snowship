using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NSettings;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{

	[UsedImplicitly]
	public class UISettingsPresenter : UIPresenter<UISettingsView> {

		private SettingsManager SettingsM => GameManager.Get<SettingsManager>();

		public UISettingsPresenter(UISettingsView view) : base(view) {
		}

		public override UniTask OnCreate() {

			SettingsM.OnSettingsChanged += UpdateButtonsForChanges;

			View.OnCancelButtonClicked += OnCancelButtonClicked;
			View.OnApplyButtonClicked += OnApplyButtonClicked;
			View.OnAcceptButtonClicked += OnAcceptButtonClicked;

			View.OnResolutionSliderChanged += OnResolutionSliderChanged;
			View.OnFullscreenToggled += OnFullscreenToggled;
			View.OnUIScaleToggled += OnUIScaleToggled;

			SetResolutionSlider();
			SetFullscreenToggle();
			SetUIScaleToggle();

			Debug.Log(SettingsM.Settings.ToString());

			return UniTask.CompletedTask;
		}

		public override void OnClose() {

			SettingsM.OnSettingsChanged -= UpdateButtonsForChanges;

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
			SettingsM.ApplySettings(SettingsM.UnappliedSettings);
		}

		private void OnAcceptButtonClicked() {
			SettingsM.ApplySettings(SettingsM.UnappliedSettings);
			GameManager.Get<UIManager>().GoBack(this);
		}

		private void SetResolutionSlider() {
			View.SetResolutionSlider(
				0,
				Screen.resolutions.Length - 1,
				Screen.resolutions.ToList().IndexOf(SettingsM.Settings.Resolution)
			);
		}

		private void SetFullscreenToggle() {
			View.SetFullscreenToggle(SettingsM.Settings.Fullscreen);
		}

		private void SetUIScaleToggle() {
			View.SetUIScaleToggle(SettingsM.Settings.ScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize);
		}

		private void OnResolutionSliderChanged(float sliderValue) {
			Resolution resolution = Screen.resolutions[Mathf.FloorToInt(sliderValue)];
			SettingsM.UnappliedSettings.SetResolution(resolution);
			View.SetResolutionText($"{resolution.width} x {resolution.height} @ {(int)resolution.refreshRateRatio.value}Hz");
			UpdateButtonsForChanges();
		}

		private void OnFullscreenToggled(bool isOn) {
			SettingsM.UnappliedSettings.SetFullscreen(isOn);
			UpdateButtonsForChanges();
		}

		private void OnUIScaleToggled(bool isOn) {
			SettingsM.UnappliedSettings.SetScaleMode(isOn ? CanvasScaler.ScaleMode.ScaleWithScreenSize : CanvasScaler.ScaleMode.ConstantPixelSize);
			UpdateButtonsForChanges();
		}

		private void UpdateButtonsForChanges() {
			bool changesExist = !SettingsM.Settings.Equals(SettingsM.UnappliedSettings);
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
