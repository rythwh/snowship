using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NInput;
using Snowship.NMap;
using Snowship.NState;
using UnityEngine;
using VContainer.Unity;
using Time = UnityEngine.Time;

namespace Snowship.NCamera
{
	[UsedImplicitly]
	public partial class CameraManager : IStartable, ITickable
	{
		private readonly ICameraQuery cameraQuery;
		private readonly ICameraWrite cameraWrite;
		private readonly IStateQuery stateQuery;
		private readonly InputManager inputM;
		private readonly IMapQuery mapQuery;
		private readonly IMapEvents mapEvents;



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

			inputM.OnInputSystemDisabled += OnInputSystemDisabled;
		}

		private void OnMapSet(Map map) {
			SetInitialCameraSettings();
			SubscribeToInputEvents();
			mapQuery.Map.LightingUpdated += OnLightingUpdated;
		}

		private void SetInitialCameraSettings()
		{
			int mapSize = mapQuery.Map.MapData.mapSize;
			cameraWrite.SetPosition(Vector2.one * mapSize / 2f);
			cameraWrite.SetZoom(CameraConstants.ZoomMax);
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
			newPosition += moveVector * (CameraConstants.MoveSpeedMultiplier * cameraQuery.CurrentZoom * Time.deltaTime);
			newPosition = new Vector2(
				Mathf.Clamp(newPosition.x, 0, mapQuery.Map.MapData.mapSize),
				Mathf.Clamp(newPosition.y, 0, mapQuery.Map.MapData.mapSize)
			);
			cameraWrite.SetPosition(newPosition);
		}

		private void ZoomCamera() {

			if (Mathf.Approximately(zoomAxis, 0)) {
				return;
			}

			float newZoom = cameraQuery.CurrentZoom;
			newZoom += (zoomAxis * CameraConstants.ZoomSpeedMultiplier) * (cameraQuery.CurrentZoom / CameraConstants.ZoomSpeedDampener);
			newZoom = Mathf.Clamp(newZoom, CameraConstants.ZoomMin, CameraConstants.ZoomMax);
			cameraWrite.SetZoom(newZoom, CameraConstants.ZoomTweenDuration);

			/*if (!zoomTaskHandle.GetAwaiter().IsCompleted && !zoomCancellationTokenSource.IsCancellationRequested) {
				zoomCancellationTokenSource.Cancel();
				zoomCancellationTokenSource.Dispose();
				zoomCancellationTokenSource = new CancellationTokenSource();
			}

			zoomTaskHandle = LMotion
				.Create(cameraQuery.CurrentZoom, newZoom, ZoomTweenDuration)
				.WithEase(Ease.OutCubic)
				.Bind(x => cameraWrite.SetZoom(x))
				.ToUniTask(zoomCancellationTokenSource.Token);*/
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
