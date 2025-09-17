using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codice.Client.Common.WebApi.Responses;

// ReSharper disable InvertIf

public class RecentAssetsWindow : EditorWindow
{
	private List<string> recentAssets = new List<string>();
	private HashSet<string> starredAssets = new HashSet<string>();
	private const string PrefsKey = "RecentAssetsHistory";
	private const string StarredPrefsKey = "StarredAssets";
	private Vector2 scrollPos;
	private int currentIndex = -1;

	[MenuItem("Window/Recent Assets Tracker")]
	public static void ShowWindow() {
		GetWindow<RecentAssetsWindow>("Recent Assets");
	}

	private void OnEnable() {
		LoadHistory();
		LoadStarred();
		EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
		EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
	}

	private void OnDisable() {
		SaveHistory();
		SaveStarred();
		EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
		EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemGUI;
	}

	private void OnGUI() {

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

		foreach (string assetPath in starredAssets.OrderByDescending(p => p)) {
			DrawAssetRow(assetPath, true);
		}

		for (int i = recentAssets.Count - 1; i >= 0; i--) {
			string assetPath = recentAssets[i];
			if (!starredAssets.Contains(assetPath)) {
				DrawAssetRow(assetPath, false);
			}
		}

		EditorGUILayout.EndScrollView();

		if (GUILayout.Button("Clear")) {
			recentAssets.Clear();
			starredAssets.Clear();
			currentIndex = -1;
			SaveHistory();
			SaveStarred();
		}
	}

	private void DrawAssetRow(string assetPath, bool isStarred) {

		Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
		bool assetExists = asset != null;

		EditorGUILayout.BeginHorizontal();

		CreateStarButton(assetPath, isStarred, assetExists);
		CreateOpenButton(asset, assetExists);
		CreateAssetIcon(assetPath);

		CreateContentButton(assetPath, assetExists);
		CreatePathDisplay(assetPath);

		EditorGUILayout.EndHorizontal();
	}

	private void CreateStarButton(string assetPath, bool isStarred, bool assetExists) {
		GUIStyle starStyle = new GUIStyle(assetExists || isStarred ? EditorStyles.iconButton : GUIStyle.none) {
			normal = {
				textColor = isStarred
					? Color.yellow
					: assetExists
						? Color.gray
						: Color.clear
			},
			fontSize = 16,
			fontStyle = FontStyle.Bold,
			fixedWidth = 20,
			fixedHeight = 20,
			alignment = TextAnchor.MiddleCenter
		};

		if (GUILayout.Button("★", starStyle)) {
			ToggleStar(assetPath, assetExists);
		}
	}

	private void CreateOpenButton(Object asset, bool assetExists) {
		Texture icon = assetExists ? EditorGUIUtility.IconContent("d_ScaleTool").image : null;
		GUIStyle style = new GUIStyle(assetExists ? EditorStyles.iconButton : EditorStyles.label) {
			fixedWidth = 20,
			fixedHeight = 20,
			alignment = TextAnchor.MiddleCenter,
		};
		if (GUILayout.Button(icon, style)) {
			AssetDatabase.OpenAsset(asset);
		}
	}

	private void CreateAssetIcon(string assetPath) {
		Texture icon = AssetDatabase.GetCachedIcon(assetPath);
		GUIStyle style = new GUIStyle(EditorStyles.label) {
			fixedWidth = 16,
			fixedHeight = 16,
			alignment = TextAnchor.MiddleCenter,
		};
		GUILayout.Label(icon, style);
	}

	private void CreateContentButton(string assetPath, bool assetExists) {

		string fileName = Path.GetFileName(assetPath);

		GUIStyle buttonStyle = new GUIStyle(EditorStyles.label) {
			alignment = TextAnchor.MiddleLeft,
			stretchWidth = false,
			normal = new GUIStyleState {
				textColor = assetExists ? Color.gray8 : Color.dimGray
			},
			hover = new GUIStyleState {
				textColor = assetExists ? Color.white : Color.dimGray
			},
			active = new GUIStyleState {
				textColor = assetExists ? Color.gray8 : Color.dimGray
			}
		};

		if (GUILayout.Button(fileName, buttonStyle)) {
			SelectAsset(recentAssets.IndexOf(assetPath));
		}
	}

	private void CreatePathDisplay(string assetPath) {

		string[] splitPath = assetPath.Split('/');
		if (splitPath.Length <= 0) {
			return;
		}
		Color colour = Color.gray4;
		splitPath = splitPath[0] switch {
			"Assets" or "Packages" => splitPath.Skip(1).ToArray(),
			_ => splitPath
		};
		splitPath = splitPath.Take(splitPath.Length - 1).ToArray();

		string displayString = string.Join(" / ", splitPath);

		GUIStyle pathStyle = new GUIStyle(EditorStyles.label) {
			fontSize = 10,
			alignment = TextAnchor.MiddleLeft,
			stretchWidth = false,
			normal = new GUIStyleState {
				textColor = colour,
			},
			padding = new RectOffset(0, 0, 2, 0),
		};

		GUILayout.Label(displayString, pathStyle);
	}

	private void OnProjectWindowItemGUI(string guid, Rect selectionRect) {
		if (Event.current.type != EventType.MouseDown || !selectionRect.Contains(Event.current.mousePosition)) {
			return;
		}

		string path = AssetDatabase.GUIDToAssetPath(guid);
		RegisterAsset(path);
	}

	private void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect) {
		if (Event.current.type != EventType.MouseDown || !selectionRect.Contains(Event.current.mousePosition)) {
			return;
		}
		Object obj = EditorUtility.InstanceIDToObject(instanceID);
		if (obj == null) {
			return;
		}
		string path = AssetDatabase.GetAssetPath(obj);
		if (!string.IsNullOrEmpty(path)) {
			RegisterAsset(path);
		}
	}

	private void RegisterAsset(string path) {
		if (string.IsNullOrEmpty(path)) {
			return;
		}
		if (recentAssets.Contains(path)) {
			recentAssets.Remove(path);
		}
		recentAssets.Add(path);
		currentIndex = recentAssets.Count - 1;
		if (recentAssets.Count > 20) {
			recentAssets.RemoveAt(0);
			currentIndex--;
		}
		SaveHistory();
		Repaint();
	}

	private void SelectAsset(int index) {
		if (index < 0 || index >= recentAssets.Count) {
			return;
		}
		string assetPath = recentAssets[index];
		Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
		if (!obj) {
			return;
		}
		Selection.activeObject = obj;
		EditorGUIUtility.PingObject(obj);
		currentIndex = index;
	}

	private void ToggleStar(string path, bool assetExists) {
		if (!assetExists && !starredAssets.Contains(path)) {
			return;
		}
		if (!starredAssets.Add(path)) {
			starredAssets.Remove(path);
		}
		SaveStarred();
		Repaint();
	}

	private void LoadHistory() {
		string data = EditorPrefs.GetString(PrefsKey, "");
		if (string.IsNullOrEmpty(data)) {
			return;
		}
		recentAssets = data.Split('|').ToList();
		currentIndex = recentAssets.Count - 1;
	}

	private void SaveHistory() {
		string data = string.Join("|", recentAssets);
		EditorPrefs.SetString(PrefsKey, data);
	}

	private void LoadStarred() {
		string data = EditorPrefs.GetString(StarredPrefsKey, "");
		if (!string.IsNullOrEmpty(data)) {
			starredAssets = new HashSet<string>(data.Split('|'));
		}
	}

	private void SaveStarred() {
		string data = string.Join("|", starredAssets);
		EditorPrefs.SetString(StarredPrefsKey, data);
	}
}
