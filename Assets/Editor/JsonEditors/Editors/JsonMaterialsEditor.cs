using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Snowship.NEntity;
using Snowship.NLife;
using Snowship.NMaterial;
using UnityEditor;
using UnityEngine;

// ReSharper disable RedundantCheckBeforeAssignment

namespace Snowship.NEditor
{
	public sealed class JsonMaterialsEditor : EditorWindow
	{
		private const string PrefKeyPath = "Snowship_MaterialsJsonPath";

		private bool uiInitialized = false;
		private string jsonPath;
		private Vector2 leftScroll;
		private Vector2 rightScroll;
		private List<JsonMaterial> materials = new();
		private readonly Dictionary<JsonMaterial, bool> validationState = new();
		private List<JsonMaterial> orderedMaterials;
		private JsonMaterial selectedMaterial = null;

		[MenuItem("Snowship/Materials JSON Editor")]
		public static void Open()
		{
			JsonMaterialsEditor window = GetWindow<JsonMaterialsEditor>(true, "Materials JSON Editor");
			window.minSize = new Vector2(1000, 600);
			window.Show();
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
			if (!uiInitialized) {
				// Draw all materials at the start to trigger validation
				using (new EditorGUILayout.HorizontalScope()) {
					foreach (JsonMaterial material in materials) {
						DrawDetail(material);
					}
				}
				uiInitialized = true;
			}

			DrawToolbar();

			EditorGUILayout.Space();

			using (new EditorGUILayout.HorizontalScope()) {
				DrawList();
				DrawDetail(selectedMaterial);
			}
		}

		private void DrawToolbar()
		{
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {

				if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(80))) {
					string picked = JsonIO.PickJsonFile("Load materials.json", jsonPath);
					if (!string.IsNullOrEmpty(picked)) {
						TryLoad(picked);
					}
				}

				if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(80))) {
					TrySave(jsonPath);
				}

				if (GUILayout.Button("Save As...", EditorStyles.toolbarButton, GUILayout.Width(80))) {
					string destination = JsonIO.SaveFile("Save materials.json", "materials", System.IO.Path.GetDirectoryName(jsonPath));
					if (!string.IsNullOrEmpty(destination)) {
						TrySave(destination);
					}
				}

				EditorGUILayout.LabelField(string.IsNullOrEmpty(jsonPath) ? "No file" : jsonPath, EditorStyles.miniLabel);
			}
		}

		private void UpdateOrderedMaterials()
		{
			materials ??= new List<JsonMaterial>();
			orderedMaterials = materials.OrderBy(m => m.ClassId).ThenBy(m => m.Id).ToList();
			foreach (JsonMaterial material in orderedMaterials) {
				material.Modifiers = material.Modifiers.OrderBy(m => m.Stat).ToList();
			}
		}

		private void DrawList()
		{
			using (new EditorGUILayout.VerticalScope("box", GUILayout.MaxWidth(300))) {

				using (new EditorGUILayout.HorizontalScope()) { // Header
					EditorGUILayout.LabelField("Materials", EditorStyles.whiteLargeLabel);
					if (GUILayout.Button(EditorGUIUtility.IconContent("d_CollabCreate Icon"), JsonEditorStyles.PlusIconStyle)) {
						AddMaterial();
					}
				}

				using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(leftScroll)) { // Materials Scroll Area
					leftScroll = scrollView.scrollPosition;

					UpdateOrderedMaterials();

					string previousClassId = string.Empty;
					foreach (JsonMaterial material in orderedMaterials) {

						// Class Header
						if (string.IsNullOrWhiteSpace(previousClassId) || !string.Equals(previousClassId, material.ClassId)) {
							previousClassId = material.ClassId;
							GUILayout.Label(previousClassId, EditorStyles.whiteLargeLabel);
						}

						// Material above Modifiers/Traits
						using (new EditorGUILayout.HorizontalScope(JsonEditorStyles.ListBoxStyle)) {
							using (new EditorGUILayout.VerticalScope()) {
								// Material Name/Button + Delete Button
								using (new EditorGUILayout.HorizontalScope("box")) {
									bool valid = JsonMaterialsEditorUtilities.IsMaterialValid(material);
									bool clicked = GUILayout.Toggle(selectedMaterial == material, string.IsNullOrEmpty(material.Id) ? "<unnamed>" : material.Id, JsonEditorStyles.ListButtonStyle(valid));
									if (clicked && selectedMaterial != material) {
										SelectMaterial(material);
									}

									if (GUILayout.Button("✕", GUILayout.Width(18))) {
										if (EditorUtility.DisplayDialog("Delete Material", $"Delete '{material.Id}'?", "Delete", "Cancel")) {
											DeleteMaterial(material);
											GUIUtility.ExitGUI();
										}
									}
								}

								using (new EditorGUILayout.HorizontalScope()) {
									// Modifiers + Traits
									if (!(material.Modifiers?.Count > 0) && !(material.Traits?.Count > 0)) {
										continue;
									}

									if (material.Modifiers is { Count: > 0 }) {
										using (new EditorGUILayout.HorizontalScope("box")) {
											foreach (JsonMaterialStatMod statMod in material.Modifiers) {
												EditorGUILayout.LabelField(JsonMaterialsEditorUtilities.GetStatModifierImage(statMod.Stat), GUILayout.MaxWidth(18), GUILayout.MaxHeight(18));
											}
										}
									}

									GUILayout.FlexibleSpace();

									if (material.Traits is { Count: > 0 }) {
										using (new EditorGUILayout.HorizontalScope("box")) {
											foreach (JsonMaterialTrait trait in material.Traits) {
												EditorGUILayout.LabelField(JsonMaterialsEditorUtilities.GetTraitImage(trait.Type), GUILayout.MaxWidth(18), GUILayout.MaxHeight(18));
											}
										}
									}
								}

								if (validationState.ContainsKey(material) && !validationState[material]) {
									EditorGUILayout.LabelField("Validation failed...", JsonEditorStyles.ErrorLabelStyle);
								}
							}
						}
					}
				}
			}
		}

		private void AddMaterial()
		{
			JsonMaterial newMaterial = new() { Id = string.Empty, ClassId = string.Empty };
			materials.Add(newMaterial);
			UpdateOrderedMaterials();
			SelectMaterial(newMaterial);
		}

		private void DeleteMaterial(JsonMaterial material)
		{
			if (selectedMaterial == material) {
				int currentIndex = orderedMaterials.IndexOf(material);
				int newSelectionIndex = orderedMaterials.Count > 1 ? (currentIndex > 0 ? currentIndex - 1 : currentIndex + 1) : -1;
				SelectMaterial(newSelectionIndex >= 0 ? orderedMaterials[newSelectionIndex] : null);
			}
			materials.Remove(material);
			orderedMaterials.Remove(material);
			UpdateOrderedMaterials();
		}

		private void SelectMaterial(JsonMaterial material)
		{
			selectedMaterial = material;

			EditorGUIUtility.editingTextField = false;
			GUIUtility.keyboardControl = 0;
			GUI.FocusControl(null);

			TrySave(jsonPath);

			Repaint();
		}

		/// <summary>
		/// Draw the properties of a single material.
		/// </summary>
		/// <returns>true if validation passed, false if validation failed.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		private bool DrawDetail(JsonMaterial material)
		{
			bool validationResult = true;
			using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(500), GUILayout.MaxWidth(500))) {
				EditorGUILayout.LabelField("Details", EditorStyles.boldLabel);
				using (new EditorGUILayout.ScrollViewScope(rightScroll)) {
					if (material != null) {
						using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(250), GUILayout.MaxWidth(250))) {

							GUIStyle textFieldStyle = new GUIStyle(EditorStyles.textField);

							string newId = EditorGUILayout.TextField("Id", material.Id, textFieldStyle);
							if (!string.Equals(newId, material.Id, StringComparison.Ordinal)) {
								material.Id = newId;
							}

							string newClass = EditorGUILayout.TextField("Class", material.ClassId, textFieldStyle);
							if (!string.Equals(newClass, material.ClassId, StringComparison.Ordinal)) {
								material.ClassId = newClass;
							}

							int newWeight = EditorGUILayout.IntField("Weight", material.Weight, textFieldStyle);
							if (newWeight != material.Weight) {
								material.Weight = newWeight;
							}

							int newVolume = EditorGUILayout.IntField("Volume", material.Volume, textFieldStyle);
							if (newVolume != material.Volume) {
								material.Volume = newVolume;
							}

							int newValue = EditorGUILayout.IntField("Value", material.Value, textFieldStyle);
							if (newValue != material.Value) {
								material.Value = newValue;
							}
						}

						EditorGUILayout.Space();

						using (new EditorGUILayout.HorizontalScope()) {
							using (new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(250))) {
								using (new EditorGUILayout.HorizontalScope()) {
									EditorGUILayout.LabelField("Stat Modifiers", EditorStyles.boldLabel);
									if (GUILayout.Button("+", GUILayout.MaxWidth(20))) {
										material.Modifiers.Add(new JsonMaterialStatMod { Stat = 0, Operation = 0, Value = 1f });
									}
								}

								List<JsonMaterialStatMod> modifiers = material.Modifiers?.ToList();
								if (modifiers != null) {
									foreach (JsonMaterialStatMod modifier in modifiers) {
										EditorGUILayout.BeginVertical("box");

										EditorGUILayout.BeginHorizontal();

										EditorGUILayout.LabelField(JsonMaterialsEditorUtilities.GetStatModifierImage(modifier.Stat), GUILayout.MaxWidth(18), GUILayout.MaxHeight(18));

										EStat newStat = (EStat)EditorGUILayout.EnumPopup(modifier.Stat);
										if (newStat != modifier.Stat) {
											modifier.Stat = newStat;
										}

										EditorGUILayout.EndHorizontal();

										EStatOperation newStatOperation = (EStatOperation)EditorGUILayout.EnumPopup(modifier.Operation);
										if (newStatOperation != modifier.Operation) {
											modifier.Operation = newStatOperation;
										}

										float value = EditorGUILayout.FloatField("Value", modifier.Value);
										if (Math.Abs(value - modifier.Value) > Mathf.Epsilon) {
											modifier.Value = value;
										}

										EditorGUILayout.BeginHorizontal();
										if (GUILayout.Button("Duplicate")) {
											JsonMaterialStatMod copy = new() { Stat = modifier.Stat, Operation = modifier.Operation, Value = modifier.Value };
											material.Modifiers.Add(copy);
										}
										if (GUILayout.Button("Remove")) {
											material.Modifiers.Remove(modifier);
											GUIUtility.ExitGUI();
										}
										EditorGUILayout.EndHorizontal();

										EditorGUILayout.EndVertical();
									}
								}
							}
							using (new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(250))) {

								using (new EditorGUILayout.HorizontalScope()) {
									EditorGUILayout.LabelField("Traits", EditorStyles.boldLabel);
									if (GUILayout.Button("+", GUILayout.MaxWidth(20))) {
										material.Traits.Add(new JsonMaterialTrait { Type = 0 });
									}
								}

								List<JsonMaterialTrait> traits = material.Traits?.ToList();
								if (traits != null) {
									foreach (JsonMaterialTrait trait in material.Traits) {
										using (new EditorGUILayout.VerticalScope("box")) {

											EditorGUILayout.BeginHorizontal();
											ETrait newTrait = (ETrait)EditorGUILayout.EnumPopup(trait.Type);
											if (newTrait != trait.Type) {
												trait.Type = newTrait;
												trait.Data = null;
											}
											if (GUILayout.Button("-", GUILayout.MaxWidth(20))) {
												material.Traits.Remove(trait);
												break;
											}
											EditorGUILayout.EndHorizontal();

											trait.Data ??= new JObject();

											JsonArgs args = new JsonArgs(trait.Data);

											switch (trait.Type) {
												case ETrait.Fuel:
													// Energy Per Unit
													JsonEditorUtilities.IntField("energyPerUnit", trait.Data, args);
													break;
												case ETrait.Craftable:
													// Craft With Entities
													validationResult &= JsonEditorUtilities.StringField("craftWithEntities", trait.Data, args);

													// Energy Required
													JsonEditorUtilities.IntField("energyRequired", trait.Data, args);

													// Resources To Craft
													validationResult &= JsonEditorUtilities.MaterialAmountField("materialsToCraft", trait.Data, args, MaterialValidationAction);

													// Time
													JsonEditorUtilities.IntField("time", trait.Data, args);

													// Output
													JsonEditorUtilities.IntField("output", trait.Data, args);

													// Active Crafting
													JsonEditorUtilities.BoolField("activeCrafting", trait.Data, args, true);
													break;
												case ETrait.Food:
													JsonEditorUtilities.IntField("nutritionPerUnit", trait.Data, args);
													break;
												case ETrait.Clothing:
													JsonEditorUtilities.EnumField<EBodySection>("appearanceKey", trait.Data, args);
													break;
												default:
													throw new ArgumentOutOfRangeException();
											}
										}
									}
								}
							}
						}
					} else {
						EditorGUILayout.HelpBox("Select a material to edit, or click '+ Add Material'", MessageType.Info);
					}
				}
			}
			validationState[material] = validationResult;
			return validationResult;
		}

		private void TryLoad(string path)
		{
			try {
				List<JsonMaterial> loaded = JsonIO.Load<List<JsonMaterial>>(path);
				materials = loaded ?? new List<JsonMaterial>();
				UpdateOrderedMaterials();
				jsonPath = path;
				EditorPrefs.SetString(PrefKeyPath, jsonPath);
				SelectMaterial(orderedMaterials.Count > 0 ? orderedMaterials[0] : null);
			} catch (Exception e) {
				Debug.LogError(e);
				EditorUtility.DisplayDialog("Load Error", e.Message, "OK");
			}
		}

		private void TrySave(string path)
		{
			if (string.IsNullOrEmpty(path)) {
				string savePath = JsonIO.SaveFile("Save materials.json", "materials", Application.dataPath);
				if (string.IsNullOrEmpty(savePath)) {
					return;
				}
				path = savePath;
			}

			try {
				JsonIO.Save(path, orderedMaterials);
				jsonPath = path;
				EditorPrefs.SetString(PrefKeyPath, jsonPath);
			} catch (Exception e) {
				Debug.LogError(e);
				EditorUtility.DisplayDialog("Save Error", e.Message, "OK");
			}
		}

		public (bool result, string message) MaterialValidationAction(string value)
		{
			if (materials.Find(m => m.Id == value) == null) {
				return (false, $"Material {value} not found in registry.");
			}
			return (true, string.Empty);
		}
	}
}
