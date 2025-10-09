using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using Snowship.NInput;
using Snowship.NMap;
using Snowship.NState;
using UnityEngine;
using VContainer.Unity;
using Time = UnityEngine.Time;

namespace Snowship.NCamera {

	public partial class CameraManager : IStartable, ITickable
	{
		private readonly ICameraQuery cameraQuery;
		private readonly ICameraWrite cameraWrite;
		private readonly IStateQuery stateQuery;
		private readonly InputManager inputM;
		private readonly IMapQuery mapQuery;
		private readonly IMapEvents mapEvents;

		private const float CameraMoveSpeedMultiplier = 1.25f;

		private const float CameraZoomSpeedMultiplier = 2.5f;
		private const float CameraZoomSpeedDampener = 5;
		private const int ZoomMin = 3;
		private const int ZoomMax = 20;
		private const float ZoomTweenDuration = 0.3f;

		private UniTask zoomTaskHandle;
		private CancellationTokenSource zoomCancellationTokenSource = new();

		public CameraManager(
			ICameraQuery cameraQuery,
			ICameraWrite cameraWrite,
			IStateQuery stateQuery,
			InputManager inputM,
			IMapQuery mapQuery,
			IMapEvents mapEvents
		) {
			this.cameraQuery = cameraQuery;
			this.cameraWrite = cameraWrite;
			this.stateQuery = stateQuery;
			this.inputM = inputM;
			this.mapQuery = mapQuery;
			this.mapEvents = mapEvents;
		}

		public void Start() {
			mapEvents.MapSet += OnMapSet;

			inputM.OnInputSystemEnabled += OnInputSystemEnabled;
			inputM.OnInputSystemDisabled += OnInputSystemDisabled;
		}

		private void OnMapSet(Map map) {
			int mapSize = mapQuery.Map.MapData.mapSize;
			cameraWrite.SetPosition(Vector2.one * mapSize / 2f, false);
			cameraWrite.SetZoom(ZoomMax);

			mapQuery.Map.LightingUpdated += OnLightingUpdated;
		}

		public void Tick() {
			if (!Mathf.Approximately(moveVector.magnitude, 0)) {
				MoveCamera();
			}
		}

		private void OnLightingUpdated(Color colour) {
			cameraWrite.SetBackgroundColour(colour);
		}

		private void MoveCamera() {
			Vector3 newPosition = cameraQuery.CurrentPosition;
			newPosition += moveVector * (CameraMoveSpeedMultiplier * cameraQuery.CurrentZoom * Time.deltaTime);
			newPosition = new Vector2(
				Mathf.Clamp(newPosition.x, 0, mapQuery.Map.MapData.mapSize),
				Mathf.Clamp(newPosition.y, 0, mapQuery.Map.MapData.mapSize)
			);
			cameraWrite.SetPosition(newPosition, false);
		}

		private void ZoomCamera() {

			if (Mathf.Approximately(zoomAxis, 0)) {
				return;
			}

			float newZoom = cameraQuery.CurrentZoom;
			newZoom += (zoomAxis * CameraZoomSpeedMultiplier) * (cameraQuery.CurrentZoom / CameraZoomSpeedDampener);
			newZoom = Mathf.Clamp(newZoom, ZoomMin, ZoomMax);

			if (!zoomTaskHandle.GetAwaiter().IsCompleted && !zoomCancellationTokenSource.IsCancellationRequested) {
				zoomCancellationTokenSource.Cancel();
				zoomCancellationTokenSource.Dispose();
				zoomCancellationTokenSource = new CancellationTokenSource();
			}

			zoomTaskHandle = LMotion
				.Create(cameraQuery.CurrentZoom, newZoom, ZoomTweenDuration)
				.WithEase(Ease.OutCubic)
				.Bind(cameraWrite.SetZoom)
				.ToUniTask(zoomCancellationTokenSource.Token);
		}

		/*// TODO Use this to improve performance on visible region blocks
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
		}*/
	}
}
