using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Snowship.NEntity;
using Snowship.NMaterial;
using UnityEditor;
using UnityEngine;

namespace Snowship.NEditor
{
	public static class JsonEntitiesEditorComponents
	{
		public static readonly Dictionary<EEntityComponent, Action<JsonEntityComponent, JsonArgs, IReadOnlyList<JsonMaterial>>> ComponentEditors = new() {
			{
				EEntityComponent.BaseStats,
				(component, args, materials) => {
					DrawBaseStatsEditor(component, args);
				}
			}, {
				EEntityComponent.BaseRecipe,
				(component, args, materials) => {
					DrawBaseRecipeEditor(component, args);
				}
			}, {
				EEntityComponent.MaterialOptions,
				(component, args, materials) => {
					DrawMaterialOptionsEditor(component, args, materials);
				}
			}, {
				EEntityComponent.Building,
				(component, args, materials) => {
					JsonEditorUtilities.IntField("width", component.Data, args, 1);
					JsonEditorUtilities.IntField("height", component.Data, args, 1);
					JsonEditorUtilities.IntField("integrity", component.Data, args, 100);
					JsonEditorUtilities.BoolField("blocks_light", component.Data, args, false);
					JsonEditorUtilities.BoolField("walkable", component.Data, args, true);
					JsonEditorUtilities.FloatField("walk_speed", component.Data, args, 1f);
					JsonEditorUtilities.FloatField("flammability", component.Data, args, 0f);
				}
			}, {
				EEntityComponent.Bed,
				(component, args, materials) => {
					JsonEditorUtilities.FloatField("comfort_amount", component.Data, args, 0f);
				}
			}, {
				EEntityComponent.Container,
				(component, args, materials) => {
					JsonEditorUtilities.IntField("weight", component.Data, args, 0);
					JsonEditorUtilities.IntField("volume", component.Data, args, 0);
				}
			}, {
				EEntityComponent.Crafting,
				(component, args, materials) => {
					EditorGUILayout.HelpBox("Crafting component has no configurable fields.", MessageType.Info);
				}
			}, {
				EEntityComponent.Plant,
				(component, args, materials) => {
					JsonEditorUtilities.IntField("growth_time_days", component.Data, args, 0);
					JsonEditorUtilities.StringField("seed_resource", component.Data, JsonEditorStyles.TextFieldStyle, args, defaultValue: string.Empty);
					JsonEditorUtilities.MaterialAmountField("harvest_materials", component.Data, materials, args);
				}
			}, {
				EEntityComponent.Quality,
				(component, args, materials) => {
					JsonEditorUtilities.FloatField("value_multiplier", component.Data, args, 1f);
					JsonEditorUtilities.FloatField("build_work_multiplier", component.Data, args, 1f);
				}
			}, {
				EEntityComponent.Rendering,
				(component, args, materials) => {
					JsonEditorUtilities.StringField("sprite_path", component.Data, null, args);
					JsonEditorUtilities.FloatField("pivot_x", component.Data, args, 0.5f);
					JsonEditorUtilities.FloatField("pivot_y", component.Data, args, 0.5f);
				}
			}
		};

		private static void DrawBaseStatsEditor(JsonEntityComponent component, JsonArgs args)
		{
			EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);

			List<string> statKeys = component.Data.Properties().Select(p => p.Name).ToList();

			using (new EditorGUILayout.HorizontalScope()) {
				if (GUILayout.Button("+ Add Stat", GUILayout.MaxWidth(100))) {
					component.Data["NewStat"] = 0f;
					GUIUtility.ExitGUI();
				}
			}

			if (statKeys.Count == 0) {
				EditorGUILayout.HelpBox("No stats defined. Click '+ Add Stat' to add one.", MessageType.Info);
				return;
			}

			List<string> keysToRemove = new();

			foreach (string key in statKeys) {
				using (new EditorGUILayout.HorizontalScope("box")) {
					string newKey = EditorGUILayout.TextField(key);
					if (newKey != key) {
						float value = component.Data[key].Value<float>();
						component.Data.Remove(key);
						component.Data[newKey] = value;
					}

					float existingValue = component.Data[newKey]?.Value<float>() ?? 0f;
					float newValue = EditorGUILayout.FloatField(existingValue);
					if (!Mathf.Approximately(newValue, existingValue)) {
						component.Data[newKey] = newValue;
					}

					if (GUILayout.Button("-", GUILayout.MaxWidth(20))) {
						keysToRemove.Add(key);
					}
				}
			}

			foreach (string key in keysToRemove) {
				component.Data.Remove(key);
			}
		}

		private static void DrawBaseRecipeEditor(JsonEntityComponent component, JsonArgs args)
		{
			EditorGUILayout.LabelField("Ingredients", EditorStyles.boldLabel);

			JArray ingredients = component.Data["ingredients"] as JArray;
			if (ingredients == null) {
				ingredients = new JArray();
				component.Data["ingredients"] = ingredients;
			}

			using (new EditorGUILayout.HorizontalScope()) {
				if (GUILayout.Button("+ Add Ingredient", GUILayout.MaxWidth(120))) {
					ingredients.Add(new JObject {
						["id"] = "",
						["amount"] = 0
					});
					GUIUtility.ExitGUI();
				}
			}

			if (ingredients.Count == 0) {
				EditorGUILayout.HelpBox("No ingredients defined. Click '+ Add Ingredient' to add one.", MessageType.Info);
				return;
			}

			for (int i = ingredients.Count - 1; i >= 0; i--) {
				JObject ingredient = ingredients[i] as JObject;
				if (ingredient == null) continue;

				using (new EditorGUILayout.VerticalScope("box")) {
					using (new EditorGUILayout.HorizontalScope()) {
						EditorGUILayout.LabelField($"Ingredient {i + 1}", EditorStyles.boldLabel);
						if (GUILayout.Button("-", GUILayout.MaxWidth(20))) {
							ingredients.RemoveAt(i);
							GUIUtility.ExitGUI();
							continue;
						}
					}

					string existingId = ingredient["id"]?.Value<string>() ?? string.Empty;
					string newId = EditorGUILayout.TextField("Resource Id", existingId);
					if (newId != existingId) {
						ingredient["id"] = newId;
					}

					int existingAmount = ingredient["amount"]?.Value<int>() ?? 0;
					int newAmount = EditorGUILayout.IntField("Amount", existingAmount);
					if (newAmount != existingAmount) {
						ingredient["amount"] = newAmount;
					}
				}
			}
		}

		private static void DrawMaterialOptionsEditor(JsonEntityComponent component, JsonArgs args, IReadOnlyList<JsonMaterial> materials)
		{
			JObject data = component.Data;

			// ---------- allowed ----------
			EditorGUILayout.LabelField("Allowed", EditorStyles.boldLabel);
			DrawStringArrayField(data, "allowed", "classes", "Allowed Classes", "class");
			DrawStringArrayField(data, "allowed", "materials", "Allowed Materials", "material");

			EditorGUILayout.Space();

			// ---------- defaults ----------
			EditorGUILayout.LabelField("Defaults", EditorStyles.boldLabel);
			JObject defaultsObj = data["defaults"] as JObject;
			if (defaultsObj == null) {
				defaultsObj = new JObject();
				data["defaults"] = defaultsObj;
			}

			string defClass = defaultsObj["class"]?.Value<string>() ?? string.Empty;
			if (materials != null) {
				List<string> classOptions = materials.Select(m => m.ClassId).Distinct().ToList();
				classOptions.Insert(0, "");
				string newDefClass = JsonEditorUtilities.StringOptionsPopup("Default Class", defClass, classOptions);
				if (newDefClass != defClass) {
					defaultsObj["class"] = newDefClass;
				}
			} else {
				string newDefClass = EditorGUILayout.TextField("Default Class", defClass);
				if (newDefClass != defClass) {
					defaultsObj["class"] = newDefClass;
				}
			}

			string defMaterial = defaultsObj["material"]?.Value<string>() ?? string.Empty;
			if (materials != null) {
				List<string> materialOptions = materials.Select(m => $"{m.ClassId}/{m.Id}").ToList();
				materialOptions.Insert(0, "");
				string newDefMaterial = JsonEditorUtilities.StringOptionsPopup("Default Material", defMaterial, materialOptions);
				if (newDefMaterial != defMaterial) {
					defaultsObj["material"] = newDefMaterial;
				}
			} else {
				string newDefMaterial = EditorGUILayout.TextField("Default Material", defMaterial);
				if (newDefMaterial != defMaterial) {
					defaultsObj["material"] = newDefMaterial;
				}
			}

			EditorGUILayout.Space();

			// ---------- cost ----------
			EditorGUILayout.LabelField("Cost Overrides", EditorStyles.boldLabel);
			JObject costObj = data["cost"] as JObject;
			if (costObj == null) {
				costObj = new JObject();
				data["cost"] = costObj;
			}

			DrawCostOverrideMap(costObj, "byClass", "By Class", materials);
			DrawCostOverrideMap(costObj, "byMaterial", "By Material", materials);
		}

		private static void DrawStringArrayField(JObject parent, string parentKey, string arrayKey, string label, string entryLabel)
		{
			JObject parentObj = parent[parentKey] as JObject;
			if (parentObj == null) {
				parentObj = new JObject();
				parent[parentKey] = parentObj;
			}

			JArray array = parentObj[arrayKey] as JArray;
			if (array == null) {
				array = new JArray();
				parentObj[arrayKey] = array;
			}

			using (new EditorGUILayout.HorizontalScope()) {
				EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
				if (GUILayout.Button("+", GUILayout.MaxWidth(20))) {
					array.Add("");
					GUIUtility.ExitGUI();
				}
			}

			for (int i = array.Count - 1; i >= 0; i--) {
				using (new EditorGUILayout.HorizontalScope("box")) {
					string existing = array[i]?.Value<string>() ?? string.Empty;
					string newValue = EditorGUILayout.TextField($"{entryLabel} {i + 1}", existing);
					if (newValue != existing) {
						array[i] = newValue;
					}
					if (GUILayout.Button("-", GUILayout.MaxWidth(20))) {
						array.RemoveAt(i);
						GUIUtility.ExitGUI();
					}
				}
			}
		}

		private static void DrawCostOverrideMap(JObject parent, string mapKey, string label, IReadOnlyList<JsonMaterial> materials)
		{
			JObject map = parent[mapKey] as JObject;
			if (map == null) {
				map = new JObject();
				parent[mapKey] = map;
			}

			using (new EditorGUILayout.HorizontalScope()) {
				EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
				if (GUILayout.Button("+ Add Override", GUILayout.MaxWidth(120))) {
					map["NewOverride"] = new JObject {
						["mode"] = "ReplaceAll",
						["recipe"] = new JArray()
					};
					GUIUtility.ExitGUI();
				}
			}

			if (map.Count == 0) {
				EditorGUILayout.HelpBox($"No {label} overrides defined.", MessageType.Info);
				return;
			}

			List<string> keysToRemove = new();
			foreach (JProperty prop in map.Properties().ToList()) {
				JObject overrideObj = prop.Value as JObject;
				if (overrideObj == null) continue;

				using (new EditorGUILayout.VerticalScope("box")) {
					using (new EditorGUILayout.HorizontalScope()) {
						string newKey = EditorGUILayout.TextField("Key", prop.Name);
						if (newKey != prop.Name) {
							JProperty renamed = new JProperty(newKey, prop.Value);
							map.Remove(prop.Name);
							map.Add(renamed);
							GUIUtility.ExitGUI();
							continue;
						}

						if (GUILayout.Button("-", GUILayout.MaxWidth(20))) {
							keysToRemove.Add(prop.Name);
							continue;
						}
					}

					// Mode
					string existingMode = overrideObj["mode"]?.Value<string>() ?? "ReplaceAll";
					string[] modes = { "ReplaceAll", "Delta" };
					int modeIndex = Array.IndexOf(modes, existingMode);
					modeIndex = modeIndex < 0 ? 0 : modeIndex;
					int newModeIndex = EditorGUILayout.Popup("Mode", modeIndex, modes);
					if (newModeIndex != modeIndex) {
						overrideObj["mode"] = modes[newModeIndex];
					}

					// Recipe (only for ReplaceAll mode)
					if (modes[newModeIndex] == "ReplaceAll") {
						DrawIngredientArray(overrideObj, "recipe", "Recipe Ingredients", materials);
					} else {
						// Add and Remove for Delta mode
						DrawIngredientArray(overrideObj, "add", "Add Ingredients", materials);
						DrawIngredientArray(overrideObj, "remove", "Remove Ingredients", materials);
					}
				}
			}

			foreach (string key in keysToRemove) {
				map.Remove(key);
			}
		}

		private static void DrawIngredientArray(JObject parent, string arrayKey, string label, IReadOnlyList<JsonMaterial> materials)
		{
			JArray array = parent[arrayKey] as JArray;
			if (array == null) {
				array = new JArray();
				parent[arrayKey] = array;
			}

			using (new EditorGUILayout.HorizontalScope()) {
				EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
				if (GUILayout.Button("+", GUILayout.MaxWidth(20))) {
					array.Add(new JObject {
						["id"] = "",
						["amount"] = 0
					});
					GUIUtility.ExitGUI();
				}
			}

			if (array.Count == 0) {
				EditorGUILayout.LabelField("(none)");
				return;
			}

			for (int i = array.Count - 1; i >= 0; i--) {
				JObject ingredient = array[i] as JObject;
				if (ingredient == null) continue;

				using (new EditorGUILayout.HorizontalScope("box")) {
					string existingId = ingredient["id"]?.Value<string>() ?? string.Empty;
					string newId;
					if (materials != null) {
						List<string> materialOptions = materials.Select(m => $"{m.ClassId}/{m.Id}").ToList();
						materialOptions.Insert(0, "");
						newId = JsonEditorUtilities.StringOptionsPopup("Id", existingId, materialOptions);
					} else {
						newId = EditorGUILayout.TextField("Id", existingId);
					}
					if (newId != existingId) {
						ingredient["id"] = newId;
					}

					int existingAmount = ingredient["amount"]?.Value<int>() ?? 0;
					int newAmount = EditorGUILayout.IntField("Amount", existingAmount, GUILayout.MaxWidth(80));
					if (newAmount != existingAmount) {
						ingredient["amount"] = newAmount;
					}

					if (GUILayout.Button("-", GUILayout.MaxWidth(20))) {
						array.RemoveAt(i);
						GUIUtility.ExitGUI();
					}
				}
			}
		}
	}
}