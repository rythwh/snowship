using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Snowship.NEntity;
using Snowship.NMaterial;
using Unity.Properties;
using UnityEditor;
using UnityEngine;

namespace Snowship.NEditor
{
	public static class JsonEditorUtilities
	{
		public static int IntField(string key, int existingValue = 0)
		{
			return EditorGUILayout.IntField(key, existingValue);
		}

		public static int IntField(string key, JObject data, JsonArgs args = null, int defaultValue = 0)
		{
			args ??= new JsonArgs(data);

			int existingValue = args.GetIntOrDefault(key, defaultValue);
			int newValue = EditorGUILayout.IntField(key, existingValue);
			if (newValue != existingValue) {
				data[key] = newValue;
			}
			return newValue;
		}

		public static float FloatField(string key, JObject data, JsonArgs args = null, float defaultValue = 0)
		{
			args ??= new JsonArgs(data);

			float existingValue = args.GetFloatOrDefault(key, defaultValue);
			float newValue = EditorGUILayout.FloatField(key, existingValue);
			if (!Mathf.Approximately(newValue, existingValue)) {
				data[key] = newValue;
			}
			return newValue;
		}

		public static string StringField(
			string key,
			string existingValue,
			GUIStyle style
		) {
			return StringField(key, existingValue, style, null).value;
		}

		public static (string value, bool valid) StringField(
			string key,
			string existingValue,
			GUIStyle style,
			Func<string, (bool result, string message)> validationAction
		) {
			(string value, bool valid) result = (existingValue, true);
			string newValue = EditorGUILayout.TextField(key, existingValue, style ?? JsonEditorStyles.TextFieldStyle);
			if (validationAction != null) {
				(bool result, string message) validationResult = validationAction(newValue);
				if (!validationResult.result) {
					EditorGUILayout.LabelField(validationResult.message, JsonEditorStyles.ErrorLabelStyle);
					result.valid = false;
					return result;
				}
			}
			if (!string.Equals(newValue, existingValue, StringComparison.OrdinalIgnoreCase)) {
				result.value = newValue;
			}
			return result;
		}

		public static bool StringField(
			string key,
			JObject data,
			GUIStyle style = null,
			JsonArgs args = null,
			Func<string, (bool result, string message)> validationAction = null,
			string defaultValue = ""
		)
		{
			args ??= new JsonArgs(data);

			string existingValue = args.GetStringOrDefault(key, defaultValue);
			string newValue = EditorGUILayout.TextField(key, existingValue, style ?? JsonEditorStyles.TextFieldStyle);
			if (validationAction != null) {
				(bool result, string message) validationResult = validationAction(newValue);
				if (!validationResult.result) {
					EditorGUILayout.LabelField(validationResult.message, JsonEditorStyles.ErrorLabelStyle);
					return false;
				}
			}
			if (!string.Equals(newValue, existingValue, StringComparison.OrdinalIgnoreCase)) {
				data[key] = newValue;
			}
			return true;
		}

		public static bool BoolField(string key, bool existingValue, GUIStyle style = null)
		{
			style ??= EditorStyles.toggle;
			bool newValue = EditorGUILayout.Toggle(key, existingValue, style);
			return newValue;
		}

		public static bool BoolField(string key, JObject data, JsonArgs args = null, bool defaultValue = false)
		{
			args ??= new JsonArgs(data);

			bool existingValue = args.GetBoolOrDefault(key, defaultValue);
			bool newValue = EditorGUILayout.Toggle(key, existingValue);
			if (newValue != existingValue) {
				data[key] = newValue;
			}
			return newValue;
		}

		public static TEnum EnumField<TEnum>(string key, TEnum existingValue = default) where TEnum : Enum
		{
			return (TEnum)EditorGUILayout.EnumPopup(existingValue);
		}

		public static void EnumField<TEnum>(string key, JObject data, JsonArgs args = null, TEnum defaultValue = default) where TEnum : Enum
		{
			args ??= new JsonArgs(data);

			TEnum existingValue = args.GetEnumValueOrDefault<TEnum>(key) ?? defaultValue;
			TEnum newValue = (TEnum)EditorGUILayout.EnumPopup(existingValue);
			if (!newValue.Equals(existingValue)) {
				data[key] = JToken.FromObject(newValue);
			}
		}

		public static string StringOptionsPopup(string label, string selectedOption, List<string> options)
		{
			int selectedIndex = options.IndexOf(selectedOption);
			selectedIndex = selectedIndex < 0 ? 0 : selectedIndex;
			selectedIndex = EditorGUILayout.Popup(label, selectedIndex, options.ToArray());
			selectedIndex = selectedIndex < 0 ? 0 : selectedIndex;
			return options[selectedIndex];
		}

		public static JsonMaterialAmount MaterialAmountField(
			string key,
			JsonMaterialAmount existingValue = null
		)
		{
			existingValue ??= new JsonMaterialAmount();

			using (new EditorGUILayout.VerticalScope("box")) {
				existingValue.Id = StringField(JsonMaterialAmount.IdPropertyName, existingValue.Id, JsonEditorStyles.TextFieldStyle);
				using (new EditorGUILayout.HorizontalScope()) {
					existingValue.Amount = IntField(JsonMaterialAmount.AmountPropertyName, existingValue.Amount);
				}
			}

			return existingValue;
		}

		public static bool MaterialAmountField(
			string key,
			JObject data,
			IReadOnlyList<JsonMaterial> materials,
			JsonArgs args = null,
			Func<string, (bool result, string message)> materialValidationAction = null,
			JsonMaterialAmount defaultValue = null
		) {
			JToken existingToken = data[key];
			if (existingToken is not JArray materialAmountArray) {
				materialAmountArray = new JArray();
			}

			using (new EditorGUILayout.HorizontalScope()) {
				EditorGUILayout.LabelField(key);
				if (GUILayout.Button("+", GUILayout.MaxWidth(20))) {
					materialAmountArray.Add(
						JObject.FromObject(
							defaultValue != null
								? new JsonMaterialAmount {
									Id = defaultValue.Id,
									Amount = defaultValue.Amount
								}
								: new JsonMaterialAmount()));
				}
			}

			for (int i = 0; i < materialAmountArray.Count; i++) {
				JToken materialAmountToken = materialAmountArray[i];
				JObject materialAmount = materialAmountToken as JObject;
				JsonArgs materialAmountArgs = new JsonArgs(materialAmount);

				using (new EditorGUILayout.VerticalScope("box")) {
					using (new EditorGUILayout.HorizontalScope()) {
						EditorGUILayout.LabelField($"Material Amount {i + 1}");
						if (GUILayout.Button("-", GUILayout.MaxWidth(20))) {
							materialAmountArray.RemoveAt(i);
							i--;
							continue;
						}
					}

					if (materials != null) {
						string existingId = materialAmount[JsonMaterialAmount.IdPropertyName].ToString();
						materialAmount[JsonMaterialAmount.IdPropertyName] = StringOptionsPopup("Material Id", existingId, materials.Select(m => $"{m.ClassId}/{m.Id}").ToList());
					}

					// if (!StringField(
					// 	JsonMaterialAmount.IdPropertyName,
					// 	materialAmount,
					// 	args: materialAmountArgs,
					// 	validationAction: materialValidationAction,
					// 	defaultValue: defaultValue?.Id ?? string.Empty
					// )) {
					// 	return false;
					// }

					IntField(
						JsonMaterialAmount.AmountPropertyName,
						materialAmount,
						materialAmountArgs,
						defaultValue?.Amount ?? 0
					);
				}
			}

			data[key] = materialAmountArray;

			return true;
		}

		public static JsonMaterialRange MaterialRangeField(
			string key,
			JsonMaterialRange existingRange = null
		)
		{
			existingRange ??= new JsonMaterialRange();

			using (new EditorGUILayout.VerticalScope("box")) {
				existingRange.MaterialId = StringField(JsonMaterialRange.MaterialIdPropertyName, existingRange.MaterialId, JsonEditorStyles.TextFieldStyle);
				using (new EditorGUILayout.HorizontalScope()) {
					existingRange.LowerLimit = IntField(JsonMaterialRange.LowerLimitPropertyName, existingRange.LowerLimit);
					existingRange.UpperLimit = IntField(JsonMaterialRange.UpperLimitPropertyName, existingRange.UpperLimit);
				}
			}

			return existingRange;
		}

		public static List<JsonMaterial> LoadMaterials()
		{
			string materialsJsonPath = EditorPrefs.GetString(JsonMaterialsEditor.PrefKeyPath, string.Empty);
			if (string.IsNullOrWhiteSpace(materialsJsonPath)) {
				throw new InvalidPathException("Unable to find materials json path in EditorPrefs.");
			}
			List<JsonMaterial> loaded = JsonIO.Load<List<JsonMaterial>>(materialsJsonPath);
			return loaded ?? new List<JsonMaterial>();
		}
	}
}
