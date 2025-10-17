using Snowship.NEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Snowship.NEditor
{
	[Serializable]
	public sealed class MaterialJson
	{
		[JsonProperty("id")] public string Id;
		[JsonProperty("class")] public string ClassId;
		[JsonProperty("mods")] public List<MaterialModJson> Mods = new();
	}

	[Serializable]
	public sealed class MaterialModJson
	{
		[JsonProperty("stat")] public string Stat;
		[JsonProperty("op")] public string Op;
		[JsonProperty("value")] public float Value;
	}

	public sealed class MaterialsEditorWindow : EditorWindow
	{
		private const string PrefKeyPath = "Snowship_MaterialsJsonPath";

		private string jsonPath;
		private Vector2 leftScroll;
		private Vector2 rightScroll;
		private List<MaterialJson> materials = new();
		private int selectedIndex = -1;

		[MenuItem("Snowship/Material Editor")]
		public static void Open()
		{
			MaterialsEditorWindow win = GetWindow<MaterialsEditorWindow>(true, "Materials Editor");
			win.minSize = new Vector2(900.0f, 500.0f);
			win.Show();
		}

		private void OnEnable()
		{
			jsonPath = EditorPrefs.GetString(PrefKeyPath, string.Empty);
			if (!string.IsNullOrWhiteSpace(jsonPath)) {
				TryLoad(jsonPath);
			}
		}

		private void OnGUI()
		{
			DrawToolbar();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			DrawList();
			DrawDetail();
			EditorGUILayout.EndHorizontal();
		}

		private void DrawToolbar()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			if (GUILayout.Button("Load", EditorStyles.toolbarButton)) {
				string picked = JsonIO.PickJsonFile("Load materials.json", jsonPath);
				if (!string.IsNullOrEmpty(picked)) {
					TryLoad(picked);
				}
			}

			if (GUILayout.Button("Save", EditorStyles.toolbarButton)) {
				TrySave(jsonPath);
			}

			if (GUILayout.Button("Save As…", EditorStyles.toolbarButton)) {
				string dest = JsonIO.SaveFile("Save materials.json", "materials", System.IO.Path.GetDirectoryName(jsonPath));
				if (!string.IsNullOrEmpty(dest)) {
					TrySave(dest);
				}
			}

			GUILayout.FlexibleSpace();
			GUIStyle style = new GUIStyle(EditorStyles.miniLabel) {
				alignment = TextAnchor.MiddleLeft,
			};
			EditorGUILayout.LabelField(string.IsNullOrEmpty(jsonPath) ? "No file" : jsonPath, style);

			EditorGUILayout.EndHorizontal();
		}

		private void DrawList()
		{
			EditorGUILayout.BeginVertical(GUILayout.Width(280.0f));
			EditorGUILayout.LabelField("Materials", EditorStyles.boldLabel);
			leftScroll = EditorGUILayout.BeginScrollView(leftScroll);

			int numMaterials = materials.OrderBy(m => m.ClassId).Count();
			string previousClassId = string.Empty;
			for (int i = 0; i < numMaterials; i++) {
				MaterialJson m = materials[i];
				if (string.IsNullOrWhiteSpace(previousClassId) || !string.Equals(previousClassId, m.ClassId)) {
					previousClassId = m.ClassId;
					GUILayout.Label(previousClassId, EditorStyles.boldLabel);
				}
				EditorGUILayout.BeginHorizontal();
				bool clicked = GUILayout.Toggle(selectedIndex == i, string.IsNullOrEmpty(m.Id) ? "<unnamed>" : m.Id, "Button");
				if (clicked && selectedIndex != i) {
					SelectMaterial(i);
				}
				if (GUILayout.Button("✕", GUILayout.Width(24.0f))) {
					if (EditorUtility.DisplayDialog("Delete Material", "Delete '" + m.Id + "'?", "Delete", "Cancel")) {
						materials.RemoveAt(i);
						if (selectedIndex == i) {
							SelectMaterial(-1);
						}
						GUIUtility.ExitGUI();
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("+ Add Material")) {
				MaterialJson mj = new() { Id = "new_material", ClassId = "wood" };
				materials.Add(mj);
				SelectMaterial(materials.Count - 1);
			}

			EditorGUILayout.EndVertical();
		}

		private void SelectMaterial(int index)
		{
			selectedIndex = index;

			EditorGUIUtility.editingTextField = false;
			GUIUtility.keyboardControl = 0;
			GUI.FocusControl(null);

			Repaint();
		}

		private void DrawDetail()
		{
			EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
			EditorGUILayout.LabelField("Details", EditorStyles.boldLabel);
			rightScroll = EditorGUILayout.BeginScrollView(rightScroll);

			if (selectedIndex >= 0 && selectedIndex < materials.Count) {
				MaterialJson m = materials[selectedIndex];
				string newId = EditorGUILayout.TextField("Id", m.Id);
				if (!string.Equals(newId, m.Id, StringComparison.Ordinal)) {
					m.Id = newId;
				}

				string newClass = EditorGUILayout.TextField("Class", m.ClassId);
				if (!string.Equals(newClass, m.ClassId, StringComparison.Ordinal)) {
					m.ClassId = newClass;
				}

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Stat Modifiers", EditorStyles.boldLabel);
				for (int i = 0; i < m.Mods.Count; i++) {
					EditorGUILayout.BeginVertical("box");

					MaterialModJson mod = m.Mods[i];

					bool validStat = Enum.TryParse(mod.Stat ?? string.Empty, true, out EStat _);

					EditorGUILayout.BeginHorizontal();
					string statStr = EditorGUILayout.TextField("Stat", mod.Stat);
					if (!string.Equals(statStr, mod.Stat, StringComparison.Ordinal)) {
						mod.Stat = statStr;
					}

					if (!validStat) {
						EditorGUILayout.LabelField("Unknown StatId", EditorStyles.helpBox, GUILayout.Width(120.0f));
					}
					EditorGUILayout.EndHorizontal();

					string opStr = EditorGUILayout.TextField("Op (Add/Mul)", mod.Op);
					if (!string.Equals(opStr, mod.Op, StringComparison.Ordinal)) {
						mod.Op = opStr;
					}

					float value = EditorGUILayout.FloatField("Value", mod.Value);
					if (Math.Abs(value - mod.Value) > Mathf.Epsilon) {
						mod.Value = value;
					}

					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("Duplicate")) {
						MaterialModJson copy = new() { Stat = mod.Stat, Op = mod.Op, Value = mod.Value };
						m.Mods.Insert(i + 1, copy);
					}
					if (GUILayout.Button("Remove")) {
						m.Mods.RemoveAt(i);
						GUIUtility.ExitGUI();
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.EndVertical();
				}

				if (GUILayout.Button("+ Add Modifier")) {
					m.Mods.Add(new MaterialModJson { Stat = nameof(EStat.Value), Op = nameof(EStatOp.Mul), Value = 1.00f });
				}
			} else {
				EditorGUILayout.HelpBox("Select a material to edit, or click '+ Add Material'", MessageType.Info);
			}

			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}

		private void TryLoad(string path)
		{
			try {
				List<MaterialJson> loaded = JsonIO.Load<List<MaterialJson>>(path);
				materials = loaded ?? new List<MaterialJson>();
				jsonPath = path;
				EditorPrefs.SetString(PrefKeyPath, jsonPath);
				SelectMaterial(materials.Count > 0 ? 0 : -1);
			} catch (Exception ex) {
				Debug.LogError(ex);
				EditorUtility.DisplayDialog("Load Error", ex.Message, "OK");
			}
		}

		private void TrySave(string path)
		{
			if (string.IsNullOrEmpty(path)) {
				string dest = JsonIO.SaveFile("Save materials.json", "materials", Application.dataPath);
				if (string.IsNullOrEmpty(dest)) {
					return;
				}
				path = dest;
			}

			try {
				JsonIO.Save(path, materials);
				jsonPath = path;
				EditorPrefs.SetString(PrefKeyPath, jsonPath);
			} catch (Exception ex) {
				Debug.LogError(ex);
				EditorUtility.DisplayDialog("Save Error", ex.Message, "OK");
			}
		}
	}
}
