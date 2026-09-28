using UnityEditor;
using UnityEngine;

public class DebugSettingsWindow : EditorWindow
{
	private const string PrefsKey = "DebugSettings";

	[MenuItem("Snowship/Snowship Debug Settings", false, 0)]
	public static void ShowWindow()
	{
		GetWindow<DebugSettingsWindow>("Snowship Debug");
	}

	private void OnGUI()
	{
		DrawHeaderRow("Debug Settings");
		DrawSettingsRow("Quick Start");
		DrawSettingsRow("Use Entity System for Buildings");
	}

	private void DrawHeaderRow(string headerTitle)
	{
		using (new EditorGUILayout.HorizontalScope())
		{
			GUILayout.Label(headerTitle, EditorStyles.boldLabel);
		}
	}

	private void DrawSettingsRow(string settingPrefsKey)
	{
		using (new EditorGUILayout.HorizontalScope())
		{
			bool playerPrefsValue = PlayerPrefs.GetInt($"{PrefsKey}/{settingPrefsKey}", 0) != 0;

			bool newValue = GUILayout.Toggle(playerPrefsValue, settingPrefsKey);

			if (newValue != playerPrefsValue)
			{
				PlayerPrefs.SetInt($"{PrefsKey}/{settingPrefsKey}", newValue ? 1 : 0);
			}

		}
	}
}
