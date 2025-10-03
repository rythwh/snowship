using UnityEditor;
using UnityEngine;

public class DebugSettingsWindow : EditorWindow
{
	private const string PrefsKey = "DebugSettings";

	[MenuItem("Tools/Snowship/Snowship Debug Settings", false, 0)]
	public static void ShowWindow() {
		GetWindow<DebugSettingsWindow>("Snowship Debug");
	}

	private void OnEnable() {
	}

	private void OnDisable() {
	}

	private void OnGUI() {
		DrawHeaderRow("Debug Settings");
		DrawSettingsRow("Quick Start");
	}

	private void DrawHeaderRow(string headerTitle) {
		EditorGUILayout.BeginHorizontal();

		GUILayout.Label(headerTitle, EditorStyles.boldLabel);

		EditorGUILayout.EndHorizontal();
	}

	private void DrawSettingsRow(string settingPrefsKey) {
		EditorGUILayout.BeginHorizontal();

		bool playerPrefsValue = PlayerPrefs.GetInt($"{PrefsKey}/{settingPrefsKey}", 0) != 0;

		bool newValue = GUILayout.Toggle(playerPrefsValue, settingPrefsKey);

		if (newValue != playerPrefsValue) {
			PlayerPrefs.SetInt($"{PrefsKey}/{settingPrefsKey}", newValue ? 1 : 0);
		}

		EditorGUILayout.EndHorizontal();
	}
}
