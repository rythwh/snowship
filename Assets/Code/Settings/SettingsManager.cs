using System;
using JetBrains.Annotations;
using Snowship.NPersistence;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;

namespace Snowship.NSettings
{
	[UsedImplicitly]
	public class SettingsManager : IStartable
	{
		private readonly SharedReferences sharedReferences;

		public SettingsState Settings { get; } = new();
		public SettingsState UnappliedSettings { get; } = new();

		public event Action OnSettingsChanged;

		public SettingsManager(SharedReferences sharedReferences) {
			this.sharedReferences = sharedReferences;
		}

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

			sharedReferences.Canvas.GetComponent<CanvasScaler>().uiScaleMode = Settings.ScaleMode;

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
