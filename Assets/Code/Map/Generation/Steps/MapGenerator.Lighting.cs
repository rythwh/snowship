using System.Collections.Generic;
using UnityEngine;

namespace Snowship.NMap.Generation
{
	public partial class MapGenerator
	{
		// TODO Convert to more conventional caching system like brightness/colour below
		internal static readonly Dictionary<int, Vector2> cachedShadowDirectionsAtTime = new Dictionary<int, Vector2>();
		internal static bool shadowDirectionsCalculated { get; private set; } = false;

		internal static void DetermineShadowDirectionsAtHour(float equatorOffset) {
			for (int h = 0; h < 24; h++) {
				float hShadow = 2f * ((h - 12f) / 24f) * (1f - Mathf.Pow(equatorOffset, 2f));
				float vShadow = Mathf.Pow(2f * ((h - 12f) / 24f), 2f) * equatorOffset + equatorOffset / 2f;
				cachedShadowDirectionsAtTime.Add(h, new Vector2(hShadow, vShadow) * 5f);
			}
			shadowDirectionsCalculated = true;
		}

		private static readonly Dictionary<int, float> cachedBrightnessLevelByTime = new Dictionary<int, float>();

		internal static float CalculateBrightnessLevelAtHour(float time) {
			int timeKey = ConvertTimeToKey(time);
			if (cachedBrightnessLevelByTime.TryGetValue(timeKey, out float brightness)) {
				return brightness;
			}
			brightness = -(1f / 144f) * Mathf.Pow((1 + (24 - (1 - time))) % 24 - 12, 2) + 1.2f;
			cachedBrightnessLevelByTime[timeKey] = brightness;
			return brightness;
		}

		private static readonly Dictionary<int, Color> cachedTileColoursByTime = new Dictionary<int, Color>();

		internal static Color GetTileColourAtHour(float time) {
			int timeKey = ConvertTimeToKey(time);
			if (cachedTileColoursByTime.TryGetValue(timeKey, out Color colour)) {
				return colour;
			}

			float r = Mathf.Clamp(Mathf.Pow(CalculateBrightnessLevelAtHour(0.4f * time + 7.2f), 10) / 5f, 0f, 1f);
			float g = Mathf.Clamp(Mathf.Pow(CalculateBrightnessLevelAtHour(0.5f * time + 6), 10) / 5f - 0.2f, 0f, 1f);
			float b = Mathf.Clamp(-1.5f * Mathf.Pow(Mathf.Cos(CalculateBrightnessLevelAtHour(2 * time + 12) / 1.5f), 3) + 1.65f * (CalculateBrightnessLevelAtHour(time) / 2f) + 0.7f, 0f, 1f);

			colour = new Color(r, g, b, 1f);
			cachedTileColoursByTime[timeKey] = colour;
			return colour;
		}

		private static int ConvertTimeToKey(float time) {
			return Mathf.RoundToInt(time * 100f);
		}
	}
}
