using System;
using Newtonsoft.Json.Linq;
using Snowship.NEntity;
using Snowship.NMaterial;
using UnityEditor;
using UnityEngine;

namespace Snowship.NEditor
{
	public static class JsonEditorUtilities
	{
		public static int IntField(string key, JObject data, JsonArgs args = null, int defaultValue = 0)
		{
			args ??= new JsonArgs(data);

			int existingValue = args.TryGetInt(key, defaultValue);
			int newValue = EditorGUILayout.IntField(key, existingValue);
			if (newValue != existingValue) {
				data[key] = newValue;
			}
			return newValue;
		}

		public static float FloatField(string key, JObject data, JsonArgs args = null, float defaultValue = 0)
		{
			args ??= new JsonArgs(data);

			float existingValue = args.TryGetFloat(key, defaultValue);
			float newValue = EditorGUILayout.FloatField(key, existingValue);
			if (!Mathf.Approximately(newValue, existingValue)) {
				data[key] = newValue;
			}
			return newValue;
		}

		public static bool StringField(string key, JObject data, JsonArgs args = null, Func<string, (bool result, string message)> validationAction = null, string defaultValue = "")
		{
			args ??= new JsonArgs(data);

			string existingValue = args.TryGetString(key, defaultValue);
			string newValue = EditorGUILayout.TextField(key, existingValue);
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

		public static bool BoolField(string key, JObject data, JsonArgs args = null, bool defaultValue = false)
		{
			args ??= new JsonArgs(data);

			bool existingValue = args.TryGetBool(key, defaultValue);
			bool newValue = EditorGUILayout.Toggle(key, existingValue);
			if (newValue != existingValue) {
				data[key] = newValue;
			}
			return newValue;
		}

		public static void EnumField<TEnum>(string key, JObject data, JsonArgs args = null, TEnum defaultValue = default) where TEnum : Enum
		{
			args ??= new JsonArgs(data);

			TEnum existingValue = args.TryGetEnum<TEnum>(key);
			TEnum newValue = (TEnum)EditorGUILayout.EnumPopup(existingValue);
			if (!newValue.Equals(existingValue)) {
				data[key] = JToken.FromObject(newValue);
			}
		}

		public static bool MaterialAmountField(
			string key,
			JObject data,
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

					if (!StringField(
						JsonMaterialAmount.IdPropertyName,
						materialAmount,
						materialAmountArgs,
						materialValidationAction,
						defaultValue?.Id ?? string.Empty
					)) {
						return false;
					}

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
	}
}
