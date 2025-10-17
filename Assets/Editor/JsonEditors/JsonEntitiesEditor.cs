using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Snowship.NEntity;
using UnityEditor;
using UnityEngine;

namespace Snowship.NEditor
{
	// ---------- JSON DTOs (Editor-only, match runtime shapes) ----------
	[Serializable]
	public sealed class JsonEntityDef
	{
		[JsonProperty("id")] public string Id;
		[JsonProperty("components")] public List<JsonComponentDef> Components = new();
	}

	[Serializable]
	public sealed class JsonComponentDef
	{
		[JsonProperty("type")] public string Type;
		[JsonProperty("data")] public JToken Data;
	}

	// Shapes for specific components we edit strongly
	[Serializable]
	public sealed class BaseStatsData
	{
		// Stored as dictionary in JSON; we keep it as JToken during edit
	}

	[Serializable]
	public sealed class IngredientJson
	{
		[JsonProperty("id")] public string Id;
		[JsonProperty("amount")] public int Amount;
	}

	[Serializable]
	public sealed class MaterialCostOverrideJson
	{
		[JsonProperty("mode")] public string Mode = "ReplaceAll"; // or "Delta"
		[JsonProperty("recipe")] public List<IngredientJson> Recipe;
		[JsonProperty("add")] public List<IngredientJson> Add;
		[JsonProperty("remove")] public List<IngredientJson> Remove;
	}

	[Serializable]
	public sealed class MaterialOptionsData
	{
		[JsonProperty("allowed")] public AllowedBlock Allowed = new();
		[JsonProperty("defaults")] public DefaultsBlock Defaults = new();
		[JsonProperty("cost")] public CostBlock Cost = new();

		[Serializable]
		public sealed class AllowedBlock
		{
			[JsonProperty("classes")] public List<string> Classes = new();
			[JsonProperty("materials")] public List<string> Materials = new();
		}

		[Serializable]
		public sealed class DefaultsBlock
		{
			[JsonProperty("class")] public string DefaultClass;
			[JsonProperty("material")] public string DefaultMaterial;
		}

		[Serializable]
		public sealed class CostBlock
		{
			[JsonProperty("byClass")] public Dictionary<string, MaterialCostOverrideJson> ByClass = new(StringComparer.OrdinalIgnoreCase);
			[JsonProperty("byMaterial")] public Dictionary<string, MaterialCostOverrideJson> ByMaterial = new(StringComparer.OrdinalIgnoreCase);
		}
	}

	public sealed class EntityDefsEditorWindow : EditorWindow
	{
		private const string PrefKeyFolder = "Snowship_EntityDefsFolder";
		private const string PrefKeyMatSuggest = "Snowship_MaterialsSuggestPath";

		private string folderPath;
		private string materialsSuggestPath;

		private Vector2 leftScroll;
		private Vector2 rightScroll;

		private readonly List<string> filePaths = new();
		private int selectedIndex = -1;
		private JsonEntityDef current;

		// Suggestions loaded from materials.json (optional)
		private readonly List<string> suggestMaterialIds = new();
		private readonly List<string> suggestClassIds = new();

		[MenuItem("Snowship/Entity Def Editor")]
		public static void Open()
		{
			EntityDefsEditorWindow win = GetWindow<EntityDefsEditorWindow>(true, "Entity Def Editor");
			win.minSize = new Vector2(1100.0f, 600.0f);
			win.Show();
		}

		private void OnEnable()
		{
			folderPath = EditorPrefs.GetString(PrefKeyFolder, string.Empty);
			materialsSuggestPath = EditorPrefs.GetString(PrefKeyMatSuggest, string.Empty);
			if (Directory.Exists(folderPath)) {
				RefreshFileList();
			}
		}

		private void OnGUI()
		{
			DrawToolbar();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			DrawFileList();
			DrawEditorPane();
			EditorGUILayout.EndHorizontal();
		}

		private void DrawToolbar()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			if (GUILayout.Button("Pick Folder", EditorStyles.toolbarButton)) {
				string picked = JsonIO.PickFolder("Select Objects Folder", folderPath);
				if (!string.IsNullOrEmpty(picked)) {
					folderPath = picked;
					EditorPrefs.SetString(PrefKeyFolder, folderPath);
					RefreshFileList();
				}
			}

			if (GUILayout.Button("New Def", EditorStyles.toolbarButton)) {
				CreateNewDef();
			}

			if (GUILayout.Button("Save", EditorStyles.toolbarButton)) {
				SaveCurrent();
			}

			if (GUILayout.Button("Save All", EditorStyles.toolbarButton)) {
				SaveAll();
			}

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Load Material Suggestions", EditorStyles.toolbarButton)) {
				string picked = JsonIO.PickJsonFile("Load materials.json", materialsSuggestPath);
				if (!string.IsNullOrEmpty(picked)) {
					materialsSuggestPath = picked;
					EditorPrefs.SetString(PrefKeyMatSuggest, materialsSuggestPath);
					LoadMaterialSuggestions(picked);
				}
			}

			EditorGUILayout.EndHorizontal();
		}

		private void DrawFileList()
		{
			EditorGUILayout.BeginVertical(GUILayout.Width(320.0f));
			EditorGUILayout.LabelField("Entity Files", EditorStyles.boldLabel);
			leftScroll = EditorGUILayout.BeginScrollView(leftScroll);

			for (int i = 0; i < filePaths.Count; i++) {
				string fileName = Path.GetFileName(filePaths[i]);
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Toggle(selectedIndex == i, fileName, "Button")) {
					if (selectedIndex != i) {
						LoadFile(i);
					}
				}
				if (GUILayout.Button("✕", GUILayout.Width(24.0f))) {
					if (EditorUtility.DisplayDialog("Delete File", "Delete '" + fileName + "'? (file will be permanently removed)", "Delete", "Cancel")) {
						File.Delete(filePaths[i]);
						AssetDatabase.Refresh();
						RefreshFileList();
						GUIUtility.ExitGUI();
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}

		private void DrawEditorPane()
		{
			EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
			rightScroll = EditorGUILayout.BeginScrollView(rightScroll);

			if (current == null) {
				EditorGUILayout.HelpBox("Pick a folder and select or create an entity def.", MessageType.Info);
				EditorGUILayout.EndScrollView();
				EditorGUILayout.EndVertical();
				return;
			}

			EditorGUILayout.LabelField("Entity Id", EditorStyles.boldLabel);
			string newId = EditorGUILayout.TextField(current.Id);
			if (!string.Equals(newId, current.Id, StringComparison.Ordinal)) {
				current.Id = newId;
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Components", EditorStyles.boldLabel);

			for (int i = 0; i < current.Components.Count; i++) {
				JsonComponentDef c = current.Components[i];
				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(c.Type, EditorStyles.boldLabel);
				if (GUILayout.Button("Remove", GUILayout.Width(80.0f))) {
					current.Components.RemoveAt(i);
					GUIUtility.ExitGUI();
				}
				EditorGUILayout.EndHorizontal();

				DrawComponentEditor(c);
				EditorGUILayout.EndVertical();
			}

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("+ BaseStats")) {
				AddBaseStats();
			}
			if (GUILayout.Button("+ BaseRecipe")) {
				AddBaseRecipe();
			}
			if (GUILayout.Button("+ MaterialOptions")) {
				AddMaterialOptions();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}

		private void DrawComponentEditor(JsonComponentDef c)
		{
			if (string.Equals(c.Type, "BaseStats", StringComparison.OrdinalIgnoreCase)) {
				DrawBaseStats(c);
				return;
			}
			if (string.Equals(c.Type, "BaseRecipe", StringComparison.OrdinalIgnoreCase)) {
				DrawBaseRecipe(c);
				return;
			}
			if (string.Equals(c.Type, "MaterialOptions", StringComparison.OrdinalIgnoreCase)) {
				DrawMaterialOptions(c);
				return;
			}

			EditorGUILayout.HelpBox("Unknown component type. Raw JSON shown below:", MessageType.None);
			string raw = c.Data != null ? c.Data.ToString(Formatting.Indented) : "{}";
			string edited = EditorGUILayout.TextArea(raw, GUILayout.MinHeight(80.0f));
			if (!string.Equals(edited, raw, StringComparison.Ordinal)) {
				try {
					c.Data = JToken.Parse(edited);
				} catch (Exception ex) {
					EditorGUILayout.HelpBox("JSON parse error: " + ex.Message, MessageType.Error);
				}
			}
		}

		private void DrawBaseStats(JsonComponentDef c)
		{
			if (c.Data == null || c.Data.Type != JTokenType.Object) {
				c.Data = new JObject();
			}

			JObject obj = (JObject)c.Data;

			Array statValues = Enum.GetValues(typeof(EStat));
			for (int i = 0; i < statValues.Length; i++) {
				EStat sid = (EStat)statValues.GetValue(i);
				string key = sid.ToString();
				float currentVal = obj.TryGetValue(key, out JToken t) ? t.Value<float>() : 0.0f;
				float newVal = EditorGUILayout.FloatField(key, currentVal);
				if (Math.Abs(newVal - currentVal) > Mathf.Epsilon) {
					obj[key] = newVal;
				}
			}
		}

		private void DrawBaseRecipe(JsonComponentDef c)
		{
			if (c.Data is not { Type: JTokenType.Array }) {
				c.Data = new JArray();
			}

			JArray arr = (JArray)c.Data;

			for (int i = 0; i < arr.Count; i++) {
				JObject row = (JObject)arr[i];
				EditorGUILayout.BeginHorizontal();
				string id = row.TryGetValue("id", out JToken tid) ? tid.Value<string>() : string.Empty;
				int amount = row.TryGetValue("amount", out JToken tam) ? tam.Value<int>() : 1;

				string newId = EditorGUILayout.TextField("Resource", id);
				int newAmt = EditorGUILayout.IntField("Amount", amount);

				if (!string.Equals(newId, id, StringComparison.Ordinal)) {
					row["id"] = newId;
				}
				if (newAmt != amount) {
					row["amount"] = newAmt;
				}

				if (GUILayout.Button("✕", GUILayout.Width(24.0f))) {
					arr.RemoveAt(i);
					GUIUtility.ExitGUI();
				}
				EditorGUILayout.EndHorizontal();
			}

			if (GUILayout.Button("+ Add Ingredient")) {
				JObject o = new() {
					["id"] = "wood",
					["amount"] = 1
				};
				arr.Add(o);
			}
		}

		private void DrawMaterialOptions(JsonComponentDef c)
		{
			if (c.Data is not { Type: JTokenType.Object }) {
				c.Data = JObject.Parse("{\n  \"allowed\": { \"classes\": [], \"materials\": [] },\n  \"defaults\": {},\n  \"cost\": { \"byClass\": {}, \"byMaterial\": {} }\n}");
			}

			JObject root = (JObject)c.Data;

			JObject allowed = root["allowed"] as JObject;
			if (allowed == null) {
				allowed = new JObject();
				root["allowed"] = allowed;
			}
			JObject defaults = root["defaults"] as JObject;
			if (defaults == null) {
				defaults = new JObject();
				root["defaults"] = defaults;
			}
			JObject cost = root["cost"] as JObject;
			if (cost == null) {
				cost = new JObject();
				root["cost"] = cost;
			}

			// Allowed
			EditorGUILayout.LabelField("Allowed", EditorStyles.boldLabel);
			DrawStringList(allowed, "classes", suggestClassIds);
			DrawStringList(allowed, "materials", suggestMaterialIds);

			// Defaults
			EditorGUILayout.LabelField("Defaults", EditorStyles.boldLabel);
			string defClass = defaults.TryGetValue("class", out JToken dc) ? dc.Value<string>() : string.Empty;
			string newDefClass = EditorGUILayout.TextField("Default Class", defClass);
			if (!string.Equals(defClass, newDefClass, StringComparison.Ordinal)) {
				defaults["class"] = newDefClass;
			}

			string defMat = defaults.TryGetValue("material", out JToken dm) ? dm.Value<string>() : string.Empty;
			string newDefMat = EditorGUILayout.TextField("Default Material", defMat);
			if (!string.Equals(defMat, newDefMat, StringComparison.Ordinal)) {
				defaults["material"] = newDefMat;
			}

			EditorGUILayout.Space();

			// Cost overrides
			EditorGUILayout.LabelField("Cost Overrides — byClass", EditorStyles.boldLabel);
			JObject byClass = cost["byClass"] as JObject;
			if (byClass == null) {
				byClass = new JObject();
				cost["byClass"] = byClass;
			}
			DrawOverrideMap(byClass);

			EditorGUILayout.LabelField("Cost Overrides — byMaterial", EditorStyles.boldLabel);
			JObject byMat = cost["byMaterial"] as JObject;
			if (byMat == null) {
				byMat = new JObject();
				cost["byMaterial"] = byMat;
			}
			DrawOverrideMap(byMat);
		}

		private void DrawOverrideMap(JObject map)
		{
			List<string> keys = new();
			foreach (JProperty p in map.Properties()) {
				keys.Add(p.Name);
			}

			foreach (string key in keys) {
				if (map[key] is not JObject ov) {
					ov = new JObject();
					map[key] = ov;
				}

				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(key, EditorStyles.boldLabel);
				if (GUILayout.Button("Rename", GUILayout.Width(70.0f))) {
					string newKey = EditorUtility.DisplayDialogComplex("Rename Key", "Enter new key in console", "OK", "Cancel", null) == 0 ? key : key; // Simplified
					// For practical rename, pop a text prompt — Unity lacks a built-in. Use a temp field below:
				}
				if (GUILayout.Button("Remove", GUILayout.Width(70.0f))) {
					map.Remove(key);
					GUIUtility.ExitGUI();
				}
				EditorGUILayout.EndHorizontal();

				string mode = ov.TryGetValue("mode", out JToken tmode) ? tmode.Value<string>() : "ReplaceAll";
				string newMode = EditorGUILayout.TextField("Mode (ReplaceAll/Delta)", mode);
				if (!string.Equals(mode, newMode, StringComparison.Ordinal)) {
					ov["mode"] = newMode;
				}

				// Recipe
				DrawIngredientArray(ov, "recipe");

				// Delta blocks
				DrawIngredientArray(ov, "add");
				DrawIngredientArray(ov, "remove");

				EditorGUILayout.EndVertical();
			}

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("+ Add Override")) {
				map["new_key"] = JObject.Parse("{ \"mode\": \"ReplaceAll\", \"recipe\": [] }");
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawIngredientArray(JObject parent, string key)
		{
			JArray arr = parent[key] as JArray;
			if (arr == null) {
				arr = new JArray();
				parent[key] = arr;
			}

			EditorGUILayout.LabelField(key, EditorStyles.miniBoldLabel);
			for (int i = 0; i < arr.Count; i++) {
				JObject row = (JObject)arr[i];
				EditorGUILayout.BeginHorizontal();
				string id = row.TryGetValue("id", out JToken tid) ? tid.Value<string>() : string.Empty;
				int amount = row.TryGetValue("amount", out JToken tam) ? tam.Value<int>() : 1;

				string newId = EditorGUILayout.TextField("Resource", id);
				int newAmt = EditorGUILayout.IntField("Amount", amount);

				if (!string.Equals(newId, id, StringComparison.Ordinal)) {
					row["id"] = newId;
				}
				if (newAmt != amount) {
					row["amount"] = newAmt;
				}
				if (GUILayout.Button("✕", GUILayout.Width(24.0f))) {
					arr.RemoveAt(i);
					GUIUtility.ExitGUI();
				}
				EditorGUILayout.EndHorizontal();
			}

			if (GUILayout.Button("+ Add Ingredient to " + key)) {
				JObject o = new() {
					["id"] = "wood",
					["amount"] = 1
				};
				arr.Add(o);
			}
		}

		private void DrawStringList(JObject parent, string key, List<string> suggestions)
		{
			JArray arr = parent[key] as JArray;
			if (arr == null) {
				arr = new JArray();
				parent[key] = arr;
			}

			EditorGUILayout.LabelField(key, EditorStyles.miniBoldLabel);
			for (int i = 0; i < arr.Count; i++) {
				string val = arr[i].Value<string>();
				EditorGUILayout.BeginHorizontal();
				string newVal = EditorGUILayout.TextField(val);
				if (!string.Equals(newVal, val, StringComparison.Ordinal)) {
					arr[i] = newVal;
				}
				if (GUILayout.Button("✕", GUILayout.Width(24.0f))) {
					arr.RemoveAt(i);
					GUIUtility.ExitGUI();
				}
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("+ Add")) {
				arr.Add("new_value");
			}

			if (suggestions is { Count: > 0 }) {
				if (GUILayout.Button("Add From Suggestions", GUILayout.Width(180.0f))) {
					GenericMenu menu = new();
					foreach (string suggestion in suggestions) {
						menu.AddItem(
							new GUIContent(suggestion),
							false,
							() => {
								arr.Add(suggestion);
							});
					}
					menu.ShowAsContext();
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		// ----- Commands -----
		private void RefreshFileList()
		{
			filePaths.Clear();
			if (!Directory.Exists(folderPath)) {
				return;
			}
			string[] files = Directory.GetFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly);
			foreach (string file in files) {
				filePaths.Add(file);
			}
			filePaths.Sort(StringComparer.OrdinalIgnoreCase);
			selectedIndex = filePaths.Count > 0 ? 0 : -1;
			if (selectedIndex >= 0) {
				LoadFile(selectedIndex);
			}
			Repaint();
		}

		private void LoadFile(int index)
		{
			try {
				JsonEntityDef def = JsonIO.Load<JsonEntityDef>(filePaths[index]);
				current = def;
				selectedIndex = index;
			} catch (Exception ex) {
				Debug.LogError(ex);
				EditorUtility.DisplayDialog("Load Error", ex.Message, "OK");
			}
		}

		private void CreateNewDef()
		{
			if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) {
				EditorUtility.DisplayDialog("Pick Folder", "Please pick an objects folder first.", "OK");
				return;
			}

			JsonEntityDef def = new() {
				Id = "new_entity",
				Components = new List<JsonComponentDef>()
			};

			string path = Path.Combine(folderPath, "new_entity.json");
			int n = 1;
			while (File.Exists(path)) {
				path = Path.Combine(folderPath, "new_entity_" + n + ".json");
				n++;
			}

			JsonIO.Save(path, def);
			RefreshFileList();
		}

		private void SaveCurrent()
		{
			if (current == null || selectedIndex < 0 || selectedIndex >= filePaths.Count) {
				return;
			}

			string path = filePaths[selectedIndex];
			try {
				JsonIO.Save(path, current);
			} catch (Exception ex) {
				Debug.LogError(ex);
				EditorUtility.DisplayDialog("Save Error", ex.Message, "OK");
			}
		}

		private void SaveAll()
		{
			for (int i = 0; i < filePaths.Count; i++) {
				if (i == selectedIndex && current != null) {
					try {
						JsonIO.Save(filePaths[i], current);
					} catch (Exception ex) {
						Debug.LogError(ex);
					}
				} else {
					// Touch others: load and re-save to format (optional). Skipped for speed.
				}
			}
		}

		private void AddBaseStats()
		{
			JObject stats = new();
			Array statValues = Enum.GetValues(typeof(EStat));
			for (int i = 0; i < statValues.Length; i++) {
				EStat sid = (EStat)statValues.GetValue(i);
				stats[sid.ToString()] = 0.0f;
			}

			JsonComponentDef comp = new() { Type = "BaseStats", Data = stats };
			current.Components.Add(comp);
		}

		private void AddBaseRecipe()
		{
			JArray arr = new() {
				JObject.FromObject(new IngredientJson { Id = "wood", Amount = 1 })
			};
			JsonComponentDef comp = new() { Type = "BaseRecipe", Data = arr };
			current.Components.Add(comp);
		}

		private void AddMaterialOptions()
		{
			JObject obj = JObject.Parse("{\n  \"allowed\": { \"classes\": [], \"materials\": [] },\n  \"defaults\": {},\n  \"cost\": { \"byClass\": {}, \"byMaterial\": {} }\n}");
			JsonComponentDef comp = new() { Type = "MaterialOptions", Data = obj };
			current.Components.Add(comp);
		}

		private void LoadMaterialSuggestions(string path)
		{
			try {
				List<MaterialJson> mats = JsonIO.Load<List<MaterialJson>>(path);
				suggestMaterialIds.Clear();
				suggestClassIds.Clear();

				HashSet<string> classes = new(StringComparer.OrdinalIgnoreCase);
				foreach (MaterialJson m in mats) {
					if (!string.IsNullOrEmpty(m.Id)) {
						suggestMaterialIds.Add(m.Id);
					}
					if (!string.IsNullOrEmpty(m.ClassId)) {
						classes.Add(m.ClassId);
					}
				}
				suggestMaterialIds.Sort(StringComparer.OrdinalIgnoreCase);
				List<string> classList = new(classes);
				classList.Sort(StringComparer.OrdinalIgnoreCase);
				suggestClassIds.AddRange(classList);
			} catch (Exception ex) {
				Debug.LogError(ex);
				EditorUtility.DisplayDialog("Material Suggest Load Error", ex.Message, "OK");
			}
		}
	}
}
