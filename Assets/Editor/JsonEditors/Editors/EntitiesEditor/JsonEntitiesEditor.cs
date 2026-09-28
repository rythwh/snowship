using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Snowship.NEntity;
using Snowship.NMaterial;
using Snowship.NSelection;
using UnityEditor;
using UnityEngine;

namespace Snowship.NEditor
{
	public sealed class JsonEntitiesEditor : EditorWindow
	{
		private const string PrefKeyPath = "Snowship_EntitiesJsonPath";

		private string jsonPath;

		private Vector2 leftScroll;
		private Vector2 rightScroll;

		private List<JsonEntity> entities;
		private List<JsonEntity> orderedEntities;
		private JsonEntity selectedEntity;
		private readonly Dictionary<JsonEntity, bool> validationState = new();

		private IReadOnlyList<JsonMaterial> materials;


		[MenuItem("Snowship/Entities JSON Editor")]
		public static void Open()
		{
			JsonEntitiesEditor window = GetWindow<JsonEntitiesEditor>(true, "Entities JSON Editor");
			window.minSize = new Vector2(1920, 1080);
			window.Show();
		}

		private void OnEnable()
		{
			materials = JsonEditorUtilities.LoadMaterials();

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
				DrawDetail(selectedEntity);
			}
		}

		private void DrawToolbar()
		{
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {

				if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(80))) {
					string picked = JsonIO.PickJsonFile("Load entities.json", jsonPath);
					if (!string.IsNullOrWhiteSpace(picked)) {
						TryLoad(picked);
					}
				}

				if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(80))) {
					TrySave(jsonPath);
				}

				if (GUILayout.Button("Save As...", EditorStyles.toolbarButton, GUILayout.Width(80))) {
					string destination = JsonIO.SaveFile("Save entities.json", "entities", System.IO.Path.GetDirectoryName(jsonPath));
					if (!string.IsNullOrWhiteSpace(destination)) {
						TrySave(destination);
					}
				}

				EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(jsonPath) ? "No file" : jsonPath, EditorStyles.miniLabel);
			}
		}

		private void UpdateOrderedEntities()
		{
			entities ??= new List<JsonEntity>();
			orderedEntities = entities.OrderBy(e => e.Category).ThenBy(e => e.SubCategory).ThenBy(e => e.Id).ToList();
			foreach (JsonEntity entity in orderedEntities) {
				entity.Components = entity.Components.OrderBy(e => e.Type).ToList();
			}
		}

		private void DrawList()
		{
			using (new EditorGUILayout.VerticalScope("box", GUILayout.MaxWidth(300))) {

				using (new EditorGUILayout.HorizontalScope()) { // Header
					EditorGUILayout.LabelField("Entities", EditorStyles.whiteLargeLabel);
					if (GUILayout.Button(EditorGUIUtility.IconContent("d_CollabCreate Icon"), JsonEditorStyles.PlusIconStyle)) {
						AddEntity();
					}
				}

				using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(leftScroll)) { // Entities Scroll Area
					leftScroll = scrollView.scrollPosition;

					UpdateOrderedEntities();

					string previousCategoryId = string.Empty;

					foreach (JsonEntity entity in orderedEntities) {

						// Category Header
						if (string.IsNullOrWhiteSpace(previousCategoryId) || !string.Equals(previousCategoryId, entity.Category)) {
							previousCategoryId = entity.Category;
							GUILayout.Label(previousCategoryId, EditorStyles.whiteLargeLabel);
						}

						// Entity above Components
						using (new EditorGUILayout.HorizontalScope(JsonEditorStyles.ListBoxStyle)) {
							using (new EditorGUILayout.VerticalScope()) {
								// Entity Name/Button + Delete Button
								using (new EditorGUILayout.HorizontalScope("box")) {
									bool valid = JsonEntitiesEditorUtilities.IsEntityValid(entity);
									bool clicked = GUILayout.Toggle(selectedEntity == entity, string.IsNullOrWhiteSpace(entity.Id) ? "<unnamed>" : entity.Id, JsonEditorStyles.ListButtonStyle(valid));
									if (clicked && selectedEntity != entity) {
										SelectEntity(entity);
									}

									if (GUILayout.Button("✕", GUILayout.Width(18))) {
										if (EditorUtility.DisplayDialog("Delete Entity", $"Delete '{entity.Id}'?", "Delete", "Cancel")) {
											DeleteEntity(entity);
											GUIUtility.ExitGUI();
										}
									}
								}

								using (new EditorGUILayout.HorizontalScope()) {
									// Components
									if (entity.Components?.Count <= 0) {
										continue;
									}

									if (entity.Components is { Count: > 0 }) {
										using (new EditorGUILayout.HorizontalScope("box")) {
											foreach (JsonEntityComponent component in entity.Components) {
												EditorGUILayout.LabelField(JsonEntitiesEditorUtilities.GetComponentImage(component.Type));
											}
										}
									}
								}

								if (validationState.ContainsKey(entity) && !validationState[entity]) {
									EditorGUILayout.LabelField("Validation failed...", JsonEditorStyles.ErrorLabelStyle);
								}
							}
						}
					}
				}
			}
		}

		private void AddEntity()
		{
			JsonEntity newEntity = new();
			// Add Rendering component by default
			newEntity.Components.Add(new JsonEntityComponent {
				Type = EEntityComponent.Rendering,
				Data = new JObject()
			});
			entities.Add(newEntity);
			UpdateOrderedEntities();
			SelectEntity(newEntity);
		}

		private void DeleteEntity(JsonEntity entity)
		{
			if (selectedEntity == entity) {
				int currentIndex = orderedEntities.IndexOf(entity);
				int newSelectionIndex = orderedEntities.Count > 1 ? (currentIndex > 0 ? currentIndex - 1 : currentIndex + 1) : -1;
				SelectEntity(newSelectionIndex >= 0 ? orderedEntities[newSelectionIndex] : null);
			}
			entities.Remove(entity);
			orderedEntities.Remove(entity);
			UpdateOrderedEntities();
		}

		private void SelectEntity(JsonEntity entity)
		{
			selectedEntity = entity;

			EditorGUIUtility.editingTextField = false;
			GUIUtility.keyboardControl = 0;
			GUI.FocusControl(null);

			TrySave(jsonPath);

			Repaint();
		}

		private bool DrawDetail(JsonEntity entity)
		{
			bool validationResult = true;
			using (new EditorGUILayout.VerticalScope(GUILayout.Width(1010))) {
				EditorGUILayout.LabelField("Details", EditorStyles.boldLabel);
				using (new EditorGUILayout.ScrollViewScope(rightScroll)) {
					if (entity == null) {
						EditorGUILayout.HelpBox("Select an entity to edit, or click '+ Add Entity'", MessageType.Info);
						return true;
					}

					using (new EditorGUILayout.VerticalScope(GUILayout.Width(500))) {

						bool fieldValidationResult;

						(entity.Id, fieldValidationResult) = JsonEditorUtilities.StringField("Id", entity.Id, JsonEditorStyles.TextFieldStyle, IdValidationAction);
						validationResult &= fieldValidationResult;

						entity.Name = JsonEditorUtilities.StringField("Name", entity.Name, JsonEditorStyles.TextFieldStyle);
						entity.Category = JsonEditorUtilities.StringField("Category", entity.Category, JsonEditorStyles.TextFieldStyle);
						entity.SubCategory = JsonEditorUtilities.StringField("Sub-category", entity.SubCategory, JsonEditorStyles.TextFieldStyle);
						entity.Description = JsonEditorUtilities.StringField("Description", entity.Description, JsonEditorStyles.TextFieldStyle);
						entity.IsAbstract = JsonEditorUtilities.BoolField("Is Abstract?", entity.IsAbstract);
					}

					EditorGUILayout.Space();

					using (new EditorGUILayout.HorizontalScope()) {
						using (new EditorGUILayout.VerticalScope(GUILayout.Width(500))) {
							using (new EditorGUILayout.HorizontalScope()) {
								EditorGUILayout.LabelField("Components", EditorStyles.boldLabel);
								if (GUILayout.Button("+", GUILayout.MaxWidth(20))) {
									entity.Components.Add(new JsonEntityComponent());
								}
							}
							List<JsonEntityComponent> components = entity.Components?.ToList();
							if (components != null) {
								// Handle Rendering component separately - show as "Pinned" at top
								JsonEntityComponent renderingComponent = components.FirstOrDefault(c => c.Type == EEntityComponent.Rendering);
								List<JsonEntityComponent> otherComponents = components.Where(c => c.Type != EEntityComponent.Rendering).ToList();

								// Draw Rendering component first (Pinned)
								if (renderingComponent != null) {
									using (new EditorGUILayout.VerticalScope("box")) {
										// Component selection bar (top of component)
										using (new EditorGUILayout.HorizontalScope()) {
											EditorGUILayout.LabelField(JsonEntitiesEditorUtilities.GetComponentImage(renderingComponent.Type), GUILayout.MaxWidth(20));

											// Show "Pinned" label instead of editable type
											EditorGUILayout.LabelField("Rendering (Pinned)", EditorStyles.boldLabel);

											// No delete button for pinned component
										}

										renderingComponent.Data ??= new JObject();

										JsonArgs args = new JsonArgs(renderingComponent.Data);

										JsonEntitiesEditorComponents.ComponentEditors.GetValueOrDefault(renderingComponent.Type)?.Invoke(renderingComponent, args, materials);
									}
								}

								// Draw other components
								foreach (JsonEntityComponent component in otherComponents) {
									using (new EditorGUILayout.VerticalScope("box")) {

										// Component selection bar (top of component)
										using (new EditorGUILayout.HorizontalScope()) {
											EditorGUILayout.LabelField(JsonEntitiesEditorUtilities.GetComponentImage(component.Type), GUILayout.MaxWidth(20));

											EEntityComponent newComponent = JsonEditorUtilities.EnumField("Type", component.Type);
											if (newComponent != component.Type) {
												component.Type = newComponent;
												component.Data = null;
											}

											if (GUILayout.Button("-", GUILayout.MaxWidth(20))) {
												entity.Components.Remove(component);
												break;
											}
										}

										component.Data ??= new JObject();

										JsonArgs args = new JsonArgs(component.Data);

										JsonEntitiesEditorComponents.ComponentEditors.GetValueOrDefault(component.Type)?.Invoke(component, args, materials);
									}
								}
							}
						}

						using (new EditorGUILayout.VerticalScope(GUILayout.Width(500))) {
							using (new EditorGUILayout.HorizontalScope()) {
								EditorGUILayout.LabelField("Variations", EditorStyles.boldLabel);
								if (GUILayout.Button("+", GUILayout.MaxWidth(20))) {
									entity.Variations.Add(new JsonEntityVariation());
								}
							}

							List<JsonEntityVariation> variations = entity.Variations?.ToList();
							if (variations != null) {
								foreach (JsonEntityVariation variation in variations) {
									using (new EditorGUILayout.VerticalScope("box")) {
										(string id, bool idValid) = JsonEditorUtilities.StringField("id", variation.Id, JsonEditorStyles.TextFieldStyle, IdValidationAction);
										if (idValid) {
											variation.Id = id;
										}
									}
								}
							}
						}
					}
				}
			}
			validationState[entity] = validationResult;
			return validationResult;
		}

		private void TryLoad(string path)
		{
			try {
				List<JsonEntity> loaded = JsonIO.Load<List<JsonEntity>>(path);
				entities = loaded ?? new List<JsonEntity>();
				UpdateOrderedEntities();
				jsonPath = path;
				EditorPrefs.SetString(PrefKeyPath, jsonPath);
				SelectEntity(orderedEntities.Count > 0 ? orderedEntities[0] : null);
			} catch (Exception e) {
				Debug.LogError(e);
				EditorUtility.DisplayDialog("Load Error", e.Message, "Ok");
			}
		}

		private void TrySave(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) {
				string savePath = JsonIO.SaveFile("Save entities.json", "entities", Application.dataPath);
				if (string.IsNullOrWhiteSpace(savePath)) {
					return;
				}
				path = savePath;
			}

			try {
				JsonIO.Save(path, orderedEntities);
				jsonPath = path;
				EditorPrefs.SetString(PrefKeyPath, jsonPath);
			} catch (Exception e) {
				Debug.LogError(e);
				EditorUtility.DisplayDialog("Save Error", e.Message, "Ok");
			}
		}

		public (bool result, string message) IdValidationAction(string value)
		{
			if (string.IsNullOrWhiteSpace(value)) {
				return (false, "Id field is empty.");
			}
			if (entities.Count(e => e.Id == value) > 1) {
				return (false, "Multiple entities with this Id exist.");
			}
			return (true, string.Empty);
		}
	}
}
