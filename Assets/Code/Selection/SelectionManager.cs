using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NMap;
using Snowship.NMap.NTile;
using Snowship.NCamera;
using Snowship.NInput;
using Snowship.NJob;
using Snowship.NState;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace Snowship.Selectable
{
	public class SelectionManager : Manager
	{
		private readonly Stack<ISelectable> selectables = new();

		private Type selectedJobType;
		private IJobDefinition selectedJobDefinition;
		private IJobParams selectedJobParams;
		private bool selecting = false;
		private SpriteRenderer selectedJobPreviewObject;
		private int rotation = 0;

		private Tile firstTile;
		private Tile secondTile;
		private Tile previousSecondTile;

		private HashSet<Tile> selectionArea = new();
		private readonly Dictionary<Tile, GameObject> selectionIndicators = new();

		private Map map;
		private Camera camera;

		public override void OnCreate() {

			InputManager inputManager = GameManager.Get<InputManager>();

			inputM.InputSystemActions.Simulation.Select.performed += OnSelectPerformed;
			inputM.InputSystemActions.Simulation.Select.canceled += OnSelectCanceled;

			inputM.InputSystemActions.Simulation.Deselect.canceled += OnDeselectCanceled;

			inputM.InputSystemActions.Simulation.Rotate.performed += OnRotatePerformed;

			stateM.OnStateChanged += OnStateChanged;

			selectedJobPreviewObject = GameManager.SharedReferences.SelectedJobPreview;
		}

		private void OnStateChanged((EState previousState, EState newState) state) {
			if (state is { previousState: EState.LoadToSimulation, newState: EState.Simulation }) {
				map = GameManager.Get<MapManager>().Map;
				camera = GameManager.Get<CameraManager>().camera;
			}
		}

		public void AddSelectable(ISelectable selectable) {
			selectables.Push(selectable);
			selectable.Select();

			Debug.Log("Selected " + selectable);
		}

		private void RemoveSelectable() {

			if (selectables.Count <= 0) {
				return;
			}

			ISelectable selectable = selectables.Pop();
			selectable.Deselect();

			Debug.Log("Deselected " + selectable);
		}

		private void OnDeselectCanceled(InputAction.CallbackContext callbackContext) {
			if (selectedJobDefinition != null) {
				ClearSelectedJob();
				return;
			}
			RemoveSelectable();
		}

		public void SetSelectedJob<TJob, TJobDefinition>()
			where TJob : class, IJob
			where TJobDefinition : class, IJobDefinition {

			ClearSelectedJob();

			selectedJobType = typeof(TJob);
			selectedJobDefinition = jobM.JobRegistry.GetJobDefinition(typeof(TJobDefinition)) as TJobDefinition;
			selectedJobParams = null;
		}

		public void SetSelectedJob<TJob, TJobDefinition>(IJobParams jobParams)
			where TJob : class, IJob
			where TJobDefinition : class, IJobDefinition {

			SetSelectedJob<TJob, TJobDefinition>();
			selectedJobParams = jobParams;
		}

		public void SetSelectedJob<TJobDefinition>(TJobDefinition jobDefinition) where TJobDefinition : class, IJobDefinition {

			ClearSelectedJob();

			selectedJobType = jobDefinition.JobType;
			selectedJobDefinition = jobDefinition;
			selectedJobParams = null;
		}

		public void SetSelectedJob<TJobDefinition>(TJobDefinition jobDefinition, IJobParams jobParams) where TJobDefinition : class, IJobDefinition {
			SetSelectedJob(jobDefinition);
			selectedJobParams = jobParams;
		}

		public void ClearSelectedJob() {
			selectedJobType = null;
			selectedJobDefinition = null;
			rotation = selectedJobParams?.SetRotation(0) ?? 0;
			selectedJobParams = null;
		}

		private void OnSelectPerformed(InputAction.CallbackContext callbackContext) {

			if (selectedJobDefinition == null) {
				return;
			}

			selecting = true;
			firstTile = GetTileFromMouseScreenPosition(Input.mousePosition);
		}

		private void OnSelectCanceled(InputAction.CallbackContext callbackContext) {
			selecting = false;

			if (selectionArea is { Count: > 0 }) {
				foreach (Tile tile in selectionArea) {
					if (selectedJobParams == null) {
						jobM.AddJob(Activator.CreateInstance(selectedJobType, tile) as IJob);
					} else {
						jobM.AddJob(Activator.CreateInstance(selectedJobType, tile, selectedJobParams) as IJob);
					}
				}
				selectionArea.Clear();
			}
			SetSelectionIndicators(null);

			firstTile = null;
			secondTile = null;
			previousSecondTile = null;
		}

		private void OnRotatePerformed(InputAction.CallbackContext context) {
			if (selectedJobParams == null) {
				return;
			}

			rotation = selectedJobParams.SetRotation(rotation + 1);
			UpdateSelectedJobPreview();
		}

		public override void OnUpdate() {
			UpdateSelectedJobPreview();
			if (selecting) {
				UpdateSelectionArea();
			}
		}

		private void UpdateSelectedJobPreview() {

			if (selectedJobDefinition == null || selecting) {
				selectedJobPreviewObject.gameObject.SetActive(false);
				return;
			}

			selectedJobPreviewObject.gameObject.SetActive(true);

			selectedJobPreviewObject.sprite = jobM.GetJobSprite(selectedJobDefinition, selectedJobParams);
			Tile overTile = GetTileFromMouseScreenPosition(Input.mousePosition);
			selectedJobPreviewObject.transform.position = overTile.obj.transform.position;
			selectedJobPreviewObject.sortingOrder = selectedJobDefinition.Layer + overTile.sr.sortingOrder + (int)SortingOrder.Selection;
		}

		private void UpdateSelectionArea() {
			previousSecondTile = secondTile;
			secondTile = GetTileFromMouseScreenPosition(Input.mousePosition);
			if (previousSecondTile == secondTile) {
				return;
			}

			selectionArea = GetSelectionArea(selectedJobDefinition);

			SetSelectionIndicators(selectedJobParams);
		}

		private HashSet<Tile> GetSelectionArea(IJobDefinition jobDefinition, IJobParams jobParams = null) {
			if (!selecting) {
				return null;
			}
			if (map == null) {
				return null;
			}
			if (jobDefinition == null) {
				return null;
			}

			Vector2Int smallerPosition = new(
				Mathf.FloorToInt(Mathf.Min(firstTile.PositionGrid.x, secondTile.PositionGrid.x)),
				Mathf.FloorToInt(Mathf.Min(firstTile.PositionGrid.y, secondTile.PositionGrid.y))
			);
			Vector2Int largerPosition = new(
				Mathf.FloorToInt(Mathf.Max(firstTile.PositionGrid.x, secondTile.PositionGrid.x)),
				Mathf.FloorToInt(Mathf.Max(firstTile.PositionGrid.y, secondTile.PositionGrid.y))
			);

			HashSet<Tile> selection = new();

			for (int y = smallerPosition.y; y <= largerPosition.y; y++) {
				for (int x = smallerPosition.x; x <= largerPosition.x; x++) {
					bool positionMatchesSelectionType = false;
					switch (jobDefinition.SelectionType) {
						case SelectionType.Full:
							positionMatchesSelectionType = true;
							break;
						case SelectionType.Outline:
							if (x == smallerPosition.x || x == largerPosition.x || y == smallerPosition.y || y == largerPosition.y) {
								positionMatchesSelectionType = true;
							}
							break;
						case SelectionType.Single:
							if (x == smallerPosition.x && y == smallerPosition.y) {
								positionMatchesSelectionType = true;
							}
							break;
						default:
							throw new ArgumentOutOfRangeException(nameof(jobDefinition.SelectionType), jobDefinition.SelectionType, null);
					}

					if (!positionMatchesSelectionType) {
						continue;
					}

					Tile tile = map.GetTileFromPosition(x, y);

					if (!(jobDefinition.SelectionConditions?.All(condition => condition(tile, jobDefinition.Layer)) ?? true)) {
						continue;
					}

					if (!(jobParams?.SelectionConditions?.All(condition => condition(tile, jobDefinition.Layer)) ?? true)) {
						continue;
					}

					selection.Add(tile);
				}
			}
			return selection;
		}

		private Tile GetTileFromMouseScreenPosition(Vector2 mousePosition) {
			return map?.GetTileFromPosition(camera.ScreenToWorldPoint(mousePosition));
		}

		private void SetSelectionIndicators(IJobParams jobParams) {
			List<Tile> selectionIndicatorsToRemove = new();

			if (selectionArea == null || selectionArea.Count == 0) {
				foreach (Tile tile in selectionIndicators.Keys) {
					Object.Destroy(selectionIndicators[tile]);
				}
				selectionIndicators.Clear();
				return;
			}

			foreach (Tile tile in selectionIndicators.Keys) {
				if (!selectionArea.Contains(tile)) {
					selectionIndicatorsToRemove.Add(tile);
				}
			}
			foreach (Tile tile in selectionIndicatorsToRemove) {
				Object.Destroy(selectionIndicators[tile]);
				selectionIndicators.Remove(tile);
			}
			foreach (Tile tile in selectionArea) {
				if (!selectionIndicators.ContainsKey(tile)) {
					GameObject selectionIndicator = Object.Instantiate(resourceM.tilePrefab, tile.obj.transform, false);
					SpriteRenderer selectionIndicatorSpriteRenderer = selectionIndicator.GetComponent<SpriteRenderer>();
					selectionIndicatorSpriteRenderer.sprite = jobM.GetJobSprite(selectedJobDefinition, selectedJobParams);
					selectionIndicatorSpriteRenderer.sortingOrder = selectedJobDefinition.Layer + tile.sr.sortingOrder + (int)SortingOrder.Selection;
					selectionIndicators.Add(tile, selectionIndicator);
				}
			}
		}
	}
}
