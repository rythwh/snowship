using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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


		[MenuItem("Snowship/Entities JSON Editor")]
		public static void Open()
		{
			JsonEntitiesEditor window = GetWindow<JsonEntitiesEditor>(true, "Entities JSON Editor");
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
									bool valid = true; // TODO JsonEntitiesEditorUtilities.IsEntityValid(entity);
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
											foreach (JsonComponent component in entity.Components) {
												// TODO EditorGUILayout.LabelField(JsonEntitiesEditorUtilities.GetComponentImage(component.Type));
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
			JsonEntity entity = new();
			entities.Add(entity);
			UpdateOrderedEntities();
			// TODO SelectEntity(entity);
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
			using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(500), GUILayout.MaxWidth(500))) {
				EditorGUILayout.LabelField("Details", EditorStyles.boldLabel);
				using (new EditorGUILayout.ScrollViewScope(rightScroll)) {
					if (entity == null) {
						EditorGUILayout.HelpBox("Select an entity to edit, or click '+ Add Entity'", MessageType.Info);
						return validationResult;
					}

					using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(250), GUILayout.MaxWidth(250))) {

						GUIStyle textFieldStyle = new GUIStyle(EditorStyles.textField);

						string newId = EditorGUILayout.TextField("Id", entity.Id, textFieldStyle);
						if (!string.Equals(newId, entity.Id, StringComparison.Ordinal)) {
							entity.Id = newId;
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
				// TODO UpdateOrderedEntities();
				jsonPath = path;
				EditorPrefs.SetString(PrefKeyPath, jsonPath);
				// TODO SelectEntity(orderedEntities.Count > 0 ? orderedEntities[0] : null);
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
	}

	public sealed class JsonEntity
	{
		[JsonProperty("id")] public string Id { get; set; }
		[JsonProperty("category")] public string Category { get; set; }
		[JsonProperty("sub_category")] public string SubCategory { get; set; }

		[JsonProperty("layer")] public int Layer { get; set; }
		[JsonProperty("build_time")] public int BuildTime { get; set; }

		[JsonProperty("integrity")] public int Integrity { get; set; }
		[JsonProperty("walk_speed")] public float WalkSpeed { get; set; }
		[JsonProperty("walkable")] public bool Walkable { get; set; }
		[JsonProperty("buildable")] public bool Buildable { get; set; }

		[JsonProperty("auto_tile")] public bool AutoTile { get; set; }
		[JsonProperty("blocks_light")] public bool BlocksLight { get; set; }

		[JsonProperty("variations")] public List<JsonEntityVariation> Variations { get; set; } = new();
		[JsonProperty("selection_modifiers")] public List<ESelectionCondition> SelectionModifiers { get; set; } = new();

		[JsonProperty("components")] public List<JsonComponent> Components { get; set; } = new();
	}

	public sealed class JsonEntityVariation
	{
		[JsonProperty("id")] public string Id { get; set; }
		[JsonProperty("unique_resources")] public List<JsonMaterialAmount> UniqueResources { get; set; } = new();
	}
}
