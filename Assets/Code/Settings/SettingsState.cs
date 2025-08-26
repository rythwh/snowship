using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NPersistence {

	[SuppressMessage("ReSharper", "ParameterHidesMember")]
	public class SettingsState {

		public Resolution Resolution { get; private set; }
		public bool Fullscreen { get; private set; }
		public CanvasScaler.ScaleMode ScaleMode { get; private set; }

		public enum Setting {
			Resolution,
			Fullscreen,
			ScaleMode
		}

		public void SetDefaults() {
			SetResolution(Screen.currentResolution);
			SetFullscreen(true);
			SetScaleMode(CanvasScaler.ScaleMode.ConstantPixelSize);
		}

		public void SetResolution(Resolution resolution) {
			Resolution = resolution;
		}

		public void SetFullscreen(bool fullscreen) {
			Fullscreen = fullscreen;
		}

		public void SetScaleMode(CanvasScaler.ScaleMode scaleMode) {
			ScaleMode = scaleMode;
		}

		public static readonly Dictionary<Setting, Func<SettingsState, string>> SettingToStringFunctions = new Dictionary<Setting, Func<SettingsState, string>> {
			{ Setting.Resolution, settingsState => $"{settingsState.Resolution.width},{settingsState.Resolution.height},{settingsState.Resolution.refreshRateRatio.value}" },
			{ Setting.Fullscreen, settingsState => settingsState.Fullscreen.ToString() },
			{ Setting.ScaleMode, settingsState => settingsState.ScaleMode.ToString() }
		};

		public static readonly Dictionary<Setting, Action<SettingsState, string>> StringToSettingFunctions = new Dictionary<Setting, Action<SettingsState, string>> {
			{
				Setting.Resolution, delegate(SettingsState settingsState, string value) {
					string[] split = value.Split(',');
					int width = int.Parse(split[0]);
					int height = int.Parse(split[1]);
					int refreshRate = (int)double.Parse(split[2]);

					for (int i = Screen.resolutions.Length - 1; i >= 0; i--) {
						Resolution resolution = Screen.resolutions[i];

						if (resolution.width != width) {
							continue;
						}

						if (resolution.height != height) {
							continue;
						}

						if (resolution.refreshRateRatio.value - refreshRate > 0.01d) {
							continue;
						}

						settingsState.SetResolution(resolution);
						return;
					}

					settingsState.SetResolution(Screen.currentResolution);
				}
			}, {
				Setting.Fullscreen, delegate(SettingsState settingsState, string value) {
					settingsState.Fullscreen = bool.Parse(value);
				}
			}, {
				Setting.ScaleMode, delegate(SettingsState settingsState, string value) {
					settingsState.ScaleMode = (CanvasScaler.ScaleMode)Enum.Parse(typeof(CanvasScaler.ScaleMode), value);
				}
			}
		};

		public override string ToString() {
			return $"{nameof(Resolution)}: {Resolution}, {nameof(Fullscreen)}: {Fullscreen}, {nameof(ScaleMode)}: {ScaleMode}";
		}

		public void CopyValuesTo(SettingsState other) {
			other.Resolution = Resolution;
			other.Fullscreen = Fullscreen;
			other.ScaleMode = ScaleMode;
		}

		public bool Equals(SettingsState other) {
			if (other == null) {
				return false;
			}

			if (!Resolution.Equals(other.Resolution)) {
				return false;
			}

			if (!Fullscreen.Equals(other.Fullscreen)) {
				return false;
			}

			if (!ScaleMode.Equals(other.ScaleMode)) {
				return false;
			}

			return true;
		}
	}
}
