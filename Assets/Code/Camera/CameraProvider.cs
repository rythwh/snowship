using System;
using System.Threading;
using JetBrains.Annotations;
using LitMotion;
using UnityEngine;

namespace Snowship.NCamera
{
	[UsedImplicitly]
	public class CameraProvider : ICameraQuery, ICameraWrite, ICameraEvents
	{
		private readonly Camera camera;

		private CancellationTokenSource moveCts = new();
		private CancellationTokenSource zoomCts = new();

		public Vector2 CurrentPosition => camera.transform.position;
		public float CurrentZoom => camera.orthographicSize;

		public event Action<Vector2, float> OnCameraPositionChanged;
		public event Action<float, Vector2> OnCameraZoomChanged;

		public CameraProvider(SharedReferences sharedReferences) {
			camera = sharedReferences.Camera;
		}

		public void SetPosition(Vector2 position, float time = 0) {

			moveCts.Cancel();
			moveCts.Dispose();
			moveCts = new CancellationTokenSource();

			if (!Mathf.Approximately(time, 0)) {
				LMotion
					.Create(camera.transform.position, (Vector3)position, time)
					.WithEase(Ease.InOutCubic)
					.WithOnComplete(() => OnCameraPositionChanged?.Invoke(CurrentPosition, CurrentZoom))
					.Bind(x => camera.transform.position = x)
					.ToUniTask(moveCts.Token);
			} else {
				camera.transform.position = position;
				OnCameraPositionChanged?.Invoke(CurrentPosition, CurrentZoom);
			}
		}

		public void SetZoom(float zoom, float time = 0)
		{
			zoomCts.Cancel();
			zoomCts.Dispose();
			zoomCts = new CancellationTokenSource();

			if (!Mathf.Approximately(time, 0)) {
				LMotion
					.Create(camera.orthographicSize, zoom, time)
					.WithEase(Ease.OutCubic)
					.WithOnComplete(() => OnCameraZoomChanged?.Invoke(CurrentZoom, CurrentPosition))
					.Bind(x => {
						camera.orthographicSize = x;
						OnCameraZoomChanged?.Invoke(CurrentZoom, CurrentPosition);
					})
					.ToUniTask(zoomCts.Token);
			} else {
				camera.orthographicSize = zoom;
				OnCameraZoomChanged?.Invoke(CurrentZoom, CurrentPosition);
			}
		}

		public void SetBackgroundColour(Color color) {
			camera.backgroundColor = color;
		}

		public Vector3 ScreenToWorld(Vector3 screenPoint) => camera.ScreenToWorldPoint(screenPoint);
		public Vector3 ScreenToViewport(Vector3 screenPoint) => camera.ScreenToViewportPoint(screenPoint);

		public Vector3 WorldToScreen(Vector3 worldPoint) => camera.WorldToScreenPoint(worldPoint);
		public Vector3 WorldToViewport(Vector3 worldPoint) => camera.WorldToViewportPoint(worldPoint);
	}
}
