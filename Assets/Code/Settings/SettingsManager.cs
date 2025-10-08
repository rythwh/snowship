using System;
using Snowship.NPersistence;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;

namespace Snowship.NSettings
{
	public class SettingsManager : IStartable
	{
		public SettingsState Settings { get; } = new();
		public SettingsState UnappliedSettings { get; } = new();

		public event Action OnSettingsChanged;

		public void Start() {
			LoadSettings();
			ApplySettings(Settings);
		}

		public void ApplySettings(SettingsState newSettings) {

			newSettings.CopyValuesTo(Settings);

			Screen.SetResolution(
				Settings.Resolution.width,
				Settings.Resolution.height,
				Settings.Fullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed,
				Settings.Resolution.refreshRateRatio
			);

			GameManager.SharedReferences.Canvas.GetComponent<CanvasScaler>().uiScaleMode = Settings.ScaleMode;

			SaveSettings();

			OnSettingsChanged?.Invoke();
		}

		private void LoadSettings() {
			foreach (SettingsState.Setting setting in SettingsState.SettingToStringFunctions.Keys) {
				string loadedSetting = PlayerPrefs.GetString(setting.ToString());
				if (string.IsNullOrEmpty(loadedSetting)) {
					continue;
				}
				SettingsState.StringToSettingFunctions[setting](Settings, loadedSetting);
			}
			Settings.CopyValuesTo(UnappliedSettings);
		}

		private void SaveSettings() {
			foreach (SettingsState.Setting setting in SettingsState.StringToSettingFunctions.Keys) {
				PlayerPrefs.SetString(setting.ToString(), SettingsState.SettingToStringFunctions[setting](Settings));
			}
		}
	}
}
