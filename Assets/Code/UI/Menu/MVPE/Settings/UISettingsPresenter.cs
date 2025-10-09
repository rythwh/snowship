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
	public class UISettingsPresenter : UIPresenter<UISettingsView>
	{
		private readonly UIManager uiM;
		private readonly SettingsManager settingM;

		public UISettingsPresenter(UISettingsView view, UIManager uiM, SettingsManager settingM) : base(view)
		{
			this.uiM = uiM;
			this.settingM = settingM;
		}

		public override UniTask OnCreate() {

			settingM.OnSettingsChanged += UpdateButtonsForChanges;

			View.OnCancelButtonClicked += OnCancelButtonClicked;
			View.OnApplyButtonClicked += OnApplyButtonClicked;
			View.OnAcceptButtonClicked += OnAcceptButtonClicked;

			View.OnResolutionSliderChanged += OnResolutionSliderChanged;
			View.OnFullscreenToggled += OnFullscreenToggled;
			View.OnUIScaleToggled += OnUIScaleToggled;

			SetResolutionSlider();
			SetFullscreenToggle();
			SetUIScaleToggle();

			Debug.Log(settingM.Settings.ToString());

			return UniTask.CompletedTask;
		}

		public override void OnClose() {

			settingM.OnSettingsChanged -= UpdateButtonsForChanges;

			View.OnCancelButtonClicked -= OnCancelButtonClicked;
			View.OnApplyButtonClicked -= OnApplyButtonClicked;
			View.OnAcceptButtonClicked -= OnAcceptButtonClicked;

			View.OnResolutionSliderChanged -= OnResolutionSliderChanged;
			View.OnFullscreenToggled -= OnFullscreenToggled;
			View.OnUIScaleToggled -= OnUIScaleToggled;
		}

		private void OnCancelButtonClicked() {
			uiM.GoBack(this);
		}

		private void OnApplyButtonClicked() {
			settingM.ApplySettings(settingM.UnappliedSettings);
		}

		private void OnAcceptButtonClicked() {
			settingM.ApplySettings(settingM.UnappliedSettings);
			uiM.GoBack(this);
		}

		private void SetResolutionSlider() {
			View.SetResolutionSlider(
				0,
				Screen.resolutions.Length - 1,
				Screen.resolutions.ToList().IndexOf(settingM.Settings.Resolution)
			);
		}

		private void SetFullscreenToggle() {
			View.SetFullscreenToggle(settingM.Settings.Fullscreen);
		}

		private void SetUIScaleToggle() {
			View.SetUIScaleToggle(settingM.Settings.ScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize);
		}

		private void OnResolutionSliderChanged(float sliderValue) {
			Resolution resolution = Screen.resolutions[Mathf.FloorToInt(sliderValue)];
			settingM.UnappliedSettings.SetResolution(resolution);
			View.SetResolutionText($"{resolution.width} x {resolution.height} @ {(int)resolution.refreshRateRatio.value}Hz");
			UpdateButtonsForChanges();
		}

		private void OnFullscreenToggled(bool isOn) {
			settingM.UnappliedSettings.SetFullscreen(isOn);
			UpdateButtonsForChanges();
		}

		private void OnUIScaleToggled(bool isOn) {
			settingM.UnappliedSettings.SetScaleMode(isOn ? CanvasScaler.ScaleMode.ScaleWithScreenSize : CanvasScaler.ScaleMode.ConstantPixelSize);
			UpdateButtonsForChanges();
		}

		private void UpdateButtonsForChanges() {
			bool changesExist = !settingM.Settings.Equals(settingM.UnappliedSettings);
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
