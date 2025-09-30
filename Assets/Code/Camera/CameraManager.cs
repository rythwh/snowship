using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using Snowship.NInput;
using Snowship.NMap;
using Snowship.NState;
using UnityEngine;
using Time = UnityEngine.Time;

namespace Snowship.NCamera {

	public partial class CameraManager : Manager {

		public Camera camera;

		private const float CameraMoveSpeedMultiplier = 1.25f;
		public event Action<Vector2> OnCameraPositionChanged;

		private const float CameraZoomSpeedMultiplier = 2.5f;
		private const float CameraZoomSpeedDampener = 5;
		private const int ZoomMin = 3;
		private const int ZoomMax = 20;
		private const float ZoomTweenDuration = 0.3f;

		private UniTask zoomTaskHandle;
		private CancellationTokenSource zoomCancellationTokenSource = new();
		public event Action<float> OnCameraZoomChanged;

		public override void OnCreate() {

			camera = GameManager.SharedReferences.Camera;

			GameManager.Get<StateManager>().OnStateChanged += OnStateChanged;

			OnInputSystemEnabled(GameManager.Get<InputManager>().InputSystemActions);
			GameManager.Get<InputManager>().OnInputSystemDisabled += OnInputSystemDisabled;
		}

		private void OnStateChanged((EState previousState, EState newState) states) {
			if (states is not { previousState: EState.MainMenu, newState: EState.LoadToSimulation }) {
				return;
			}

			int mapSize = GameManager.Get<MapManager>().Map.MapData.mapSize;
			SetCameraPosition(Vector2.one * mapSize / 2f);
			SetCameraZoom(5);
		}

		public override void OnUpdate() {
			if (!Mathf.Approximately(moveVector.magnitude, 0)) {
				MoveCamera();
			}
		}

		public Vector2 GetCameraPosition() {
			return camera.transform.position;
		}

		public void SetCameraPosition(Vector2 position) {
			camera.transform.position = position;
		}

		public void SetCameraZoom(float zoom) {
			camera.orthographicSize = zoom;
		}

		private void MoveCamera() {

			camera.transform.Translate(moveVector * (CameraMoveSpeedMultiplier * camera.orthographicSize * Time.deltaTime));
			camera.transform.position = new Vector2(
				Mathf.Clamp(camera.transform.position.x, 0, GameManager.Get<MapManager>().Map.MapData.mapSize),
				Mathf.Clamp(camera.transform.position.y, 0, GameManager.Get<MapManager>().Map.MapData.mapSize)
			);
			OnCameraPositionChanged?.Invoke(camera.transform.position);
		}

		private void ZoomCamera() {

			if (Mathf.Approximately(zoomAxis, 0)) {
				return;
			}

			float currentZoom = camera.orthographicSize;
			float newZoom = currentZoom + (zoomAxis * CameraZoomSpeedMultiplier) * (camera.orthographicSize / CameraZoomSpeedDampener);
			newZoom = Mathf.Clamp(newZoom, ZoomMin, ZoomMax);

			if (!zoomTaskHandle.GetAwaiter().IsCompleted && !zoomCancellationTokenSource.IsCancellationRequested) {
				zoomCancellationTokenSource.Cancel();
				zoomCancellationTokenSource.Dispose();
				zoomCancellationTokenSource = new CancellationTokenSource();
			}

			zoomTaskHandle = LMotion
				.Create(camera.orthographicSize, newZoom, ZoomTweenDuration)
				.WithEase(Ease.OutCubic)
				.WithOnComplete(() => OnCameraZoomChanged?.Invoke(newZoom))
				.Bind(x => camera.orthographicSize = x)
				.ToUniTask(zoomCancellationTokenSource.Token);
		}

		// TODO Use this to improve performance on visible region blocks
		public RectInt CalculateCameraWorldRect() {
			Vector2Int bottomLeftCorner = Vector2Int.FloorToInt(camera.ViewportToWorldPoint(new Vector3(0, 0, 0)));
			Vector2Int topRightCorner = Vector2Int.CeilToInt(camera.ViewportToWorldPoint(new Vector3(1, 1, 1)));

			int xMin = bottomLeftCorner.x;
			int yMin = bottomLeftCorner.y;

			int width = topRightCorner.x - bottomLeftCorner.x;
			int height = topRightCorner.y - bottomLeftCorner.y;

			return new RectInt(
				xMin,
				yMin,
				width,
				height
			);
		}
	}
}
