using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Snowship.NEntity;
using Snowship.NLife;
using Snowship.NMaterial;
using Snowship.NUtilities;
using UnityEditor;
using UnityEngine;

// ReSharper disable RedundantCheckBeforeAssignment

namespace Snowship.NEditor
{
	public sealed class MaterialsEditorWindow : EditorWindow
	{
		private const string PrefKeyPath = "Snowship_MaterialsJsonPath";

		private string jsonPath;
		private Vector2 leftScroll;
		private Vector2 rightScroll;
		private List<JsonMaterial> materials = new();
		private List<JsonMaterial> orderedMaterials;
		private JsonMaterial selectedMaterial = null;

		[MenuItem("Snowship/Materials JSON Editor")]
		public static void Open()
		{
			MaterialsEditorWindow win = GetWindow<MaterialsEditorWindow>(true, "Materials JSON Editor");
			win.minSize = new Vector2(1000, 600);
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

			using (new EditorGUILayout.HorizontalScope()) {
				DrawList();
				DrawDetail();
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

				if (GUILayout.Button("Save As…", EditorStyles.toolbarButton, GUILayout.Width(80))) {
					string dest = JsonIO.SaveFile("Save materials.json", "materials", System.IO.Path.GetDirectoryName(jsonPath));
					if (!string.IsNullOrEmpty(dest)) {
						TrySave(dest);
					}
				}

				EditorGUILayout.LabelField(string.IsNullOrEmpty(jsonPath) ? "No file" : jsonPath, EditorStyles.miniLabel);
			}
		}

		private bool IsMaterialValid(JsonMaterial material)
		{
			return
				!string.IsNullOrWhiteSpace(material.Id)
				&& !string.IsNullOrWhiteSpace(material.ClassId);
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
					GUIStyle plusIconStyle = new GUIStyle(EditorStyles.iconButton) {
						imagePosition = ImagePosition.ImageOnly,
						alignment = TextAnchor.MiddleCenter,
						fixedWidth = 22,
						fixedHeight = 22
					};
					if (GUILayout.Button(EditorGUIUtility.IconContent("d_CollabCreate Icon"), plusIconStyle)) {
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

						// Full Material Box
						GUIStyle materialBoxStyle = new GUIStyle("box") {
							normal = {
								background = Texture2D.grayTexture
							}
						};
						// Material above Modifiers/Traits
						using (new EditorGUILayout.HorizontalScope(materialBoxStyle)) {
							using (new EditorGUILayout.VerticalScope()) {
								// Material Name/Button + Delete Button
								using (new EditorGUILayout.HorizontalScope("box")) {
									bool valid = IsMaterialValid(material);
									GUIStyle materialButtonStyle = new GUIStyle(EditorStyles.miniButton) {
										alignment = TextAnchor.MiddleLeft,
										normal = {
											textColor = valid ? Color.white : Color.softRed
										},
										focused = {
											textColor = valid ? Color.white : Color.darkRed
										}
									};
									bool clicked = GUILayout.Toggle(selectedMaterial == material, string.IsNullOrEmpty(material.Id) ? "<unnamed>" : material.Id, materialButtonStyle);
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
												EditorGUILayout.LabelField(GetStatModifierImage(statMod.Stat), GUILayout.MaxWidth(18), GUILayout.MaxHeight(18));
											}
										}
									}

									GUILayout.FlexibleSpace();

									if (material.Traits is { Count: > 0 }) {
										using (new EditorGUILayout.HorizontalScope("box")) {
											foreach (JsonMaterialTrait trait in material.Traits) {
												EditorGUILayout.LabelField(GetTraitImage(trait.Type), GUILayout.MaxWidth(18), GUILayout.MaxHeight(18));
											}
										}
									}
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

		private void DrawDetail()
		{
			using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(500), GUILayout.MaxWidth(500))) {
				EditorGUILayout.LabelField("Details", EditorStyles.boldLabel);
				using (new EditorGUILayout.ScrollViewScope(rightScroll)) {
					if (selectedMaterial != null) {
						using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(250), GUILayout.MaxWidth(250))) {

							GUIStyle textFieldStyle = new GUIStyle(EditorStyles.textField);

							string newId = EditorGUILayout.TextField("Id", selectedMaterial.Id, textFieldStyle);
							if (!string.Equals(newId, selectedMaterial.Id, StringComparison.Ordinal)) {
								selectedMaterial.Id = newId;
							}

							string newClass = EditorGUILayout.TextField("Class", selectedMaterial.ClassId, textFieldStyle);
							if (!string.Equals(newClass, selectedMaterial.ClassId, StringComparison.Ordinal)) {
								selectedMaterial.ClassId = newClass;
							}

							int newWeight = EditorGUILayout.IntField("Weight", selectedMaterial.Weight, textFieldStyle);
							if (newWeight != selectedMaterial.Weight) {
								selectedMaterial.Weight = newWeight;
							}

							int newVolume = EditorGUILayout.IntField("Volume", selectedMaterial.Volume, textFieldStyle);
							if (newVolume != selectedMaterial.Volume) {
								selectedMaterial.Volume = newVolume;
							}

							int newValue = EditorGUILayout.IntField("Value", selectedMaterial.Value, textFieldStyle);
							if (newValue != selectedMaterial.Value) {
								selectedMaterial.Value = newValue;
							}
						}

						EditorGUILayout.Space();

						using (new EditorGUILayout.HorizontalScope()) {
							using (new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(250))) {
								using (new EditorGUILayout.HorizontalScope()) {
									EditorGUILayout.LabelField("Stat Modifiers", EditorStyles.boldLabel);
									if (GUILayout.Button("+", GUILayout.MaxWidth(20))) {
										selectedMaterial.Modifiers.Add(new JsonMaterialStatMod { Stat = 0, Operation = 0, Value = 1f });
									}
								}

								List<JsonMaterialStatMod> modifiers = selectedMaterial.Modifiers?.ToList();
								if (modifiers != null) {
									foreach (JsonMaterialStatMod modifier in modifiers) {
										EditorGUILayout.BeginVertical("box");

										EditorGUILayout.BeginHorizontal();

										EditorGUILayout.LabelField(GetStatModifierImage(modifier.Stat), GUILayout.MaxWidth(18), GUILayout.MaxHeight(18));

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
											selectedMaterial.Modifiers.Add(copy);
										}
										if (GUILayout.Button("Remove")) {
											selectedMaterial.Modifiers.Remove(modifier);
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
										selectedMaterial.Traits.Add(new JsonMaterialTrait { Type = 0 });
									}
								}

								List<JsonMaterialTrait> traits = selectedMaterial.Traits?.ToList();
								if (traits != null) {
									foreach (JsonMaterialTrait trait in selectedMaterial.Traits) {
										ETrait newTrait = (ETrait)EditorGUILayout.EnumPopup(trait.Type);
										if (newTrait != trait.Type) {
											trait.Type = newTrait;
											trait.Data = null;
										}

										trait.Data ??= new JObject();

										JsonArgs args = new JsonArgs(trait.Data);

										switch (trait.Type) {
											case ETrait.Fuel:
												// Energy Per Unit
												JsonEditorUtilities.IntField("energyPerUnit", trait.Data, args);
												break;
											case ETrait.Craftable:
												// Craft With Entities
												JsonEditorUtilities.StringField("craftWithEntities", trait.Data, args);

												// Energy Required
												JsonEditorUtilities.IntField("energyRequired", trait.Data, args);

												// Resources To Craft
												JsonEditorUtilities.MaterialAmountField("materialsToCraft", trait.Data, args);

												// Time
												JsonEditorUtilities.IntField("time", trait.Data, args);

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
					} else {
						EditorGUILayout.HelpBox("Select a material to edit, or click '+ Add Material'", MessageType.Info);
					}

				}
			}
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
				JsonIO.Save(path, orderedMaterials);
				jsonPath = path;
				EditorPrefs.SetString(PrefKeyPath, jsonPath);
			} catch (Exception ex) {
				Debug.LogError(ex);
				EditorUtility.DisplayDialog("Save Error", ex.Message, "OK");
			}
		}

		private GUIContent GetStatModifierImage(EStat stat)
		{
			return stat switch {
				EStat.BuildTime => EditorGUIUtility.IconContent("UnityEditor.AnimationWindow@2x"),
				EStat.Value => EditorGUIUtility.IconContent("d_LightProbes Icon"),
				EStat.WalkSpeed => EditorGUIUtility.IconContent("d_NavMeshAgent Icon"),
				EStat.Integrity => EditorGUIUtility.IconContent("Animation.Record@2x"),
				EStat.Flammability => EditorGUIUtility.IconContent("d_VisualEffectAsset Icon"),
				EStat.Insulation => EditorGUIUtility.IconContent("d_SpeedTreeWindAsset Icon"),
				EStat.Fuel => EditorGUIUtility.IconContent("d_ParticleSystemForceField Icon"),
				_ => EditorGUIUtility.IconContent("d_Help@2x")
			};
		}

		private GUIContent GetTraitImage(ETrait trait)
		{
			return trait switch {
				ETrait.Fuel => EditorGUIUtility.IconContent("d_ParticleSystemForceField Icon"),
				ETrait.Craftable => EditorGUIUtility.IconContent("BuildProfile Icon"),
				ETrait.Food => EditorGUIUtility.IconContent("d_PlatformEffector2D Icon"),
				ETrait.Clothing => EditorGUIUtility.IconContent("Cloth Icon"),
				_ => EditorGUIUtility.IconContent("d_Help@2x")
			};
		}
	}
}
