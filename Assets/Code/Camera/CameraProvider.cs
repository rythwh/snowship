using System;
using System.Threading;
using LitMotion;
using UnityEngine;

namespace Snowship.NCamera
{
	public class CameraProvider : ICameraQuery, ICameraWrite, ICameraEvents
	{
		private readonly Camera camera;

		private CancellationTokenSource moveCts = new();

		public Vector2 CurrentPosition => camera.transform.position;
		public float CurrentZoom => camera.orthographicSize;

		public event Action<Vector2, float> OnCameraPositionChanged;
		public event Action<float, Vector2> OnCameraZoomChanged;

		public CameraProvider(SharedReferences sharedReferences) {
			camera = sharedReferences.Camera;
		}

		public void SetPosition(Vector2 position, bool animate = true) {

			moveCts.Cancel();
			moveCts.Dispose();
			moveCts = new CancellationTokenSource();

			if (animate) {
				LMotion
					.Create(camera.transform.position, (Vector3)position, 2)
					.WithEase(Ease.InOutCubic)
					.WithOnComplete(() => OnCameraPositionChanged?.Invoke(CurrentPosition, CurrentZoom))
					.Bind(x => camera.transform.position = x)
					.ToUniTask(moveCts.Token);
			} else {
				camera.transform.position = position;
				OnCameraPositionChanged?.Invoke(CurrentPosition, CurrentZoom);
			}
		}

		public void SetZoom(float zoom) {
			camera.orthographicSize = zoom;
			OnCameraZoomChanged?.Invoke(CurrentZoom, CurrentPosition);
		}

		public void SetBackgroundColour(Color color) {
			camera.backgroundColor = color;
		}

		public Vector3 ScreenToWorld(Vector3 screenPoint) => camera.ScreenToWorldPoint(screenPoint);
		public Vector3 ScreenToViewport(Vector3 screenPoint) => camera.ScreenToViewportPoint(screenPoint);

		public Vector3 WorldToScreen(Vector3 worldPoint) => camera.WorldToScreenPoint(worldPoint);
		public Vector3 WorldToViewport(Vector3 worldPoint) => camera.WorldToViewportPoint(worldPoint);
	}

	public interface ICameraQuery
	{
		Vector2 CurrentPosition { get; }
		float CurrentZoom { get; }

		Vector3 ScreenToWorld(Vector3 screenPoint);
		Vector3 ScreenToViewport(Vector3 screenPoint);

		Vector3 WorldToScreen(Vector3 worldPosition);
		Vector3 WorldToViewport(Vector3 worldPosition);
	}

	public interface ICameraWrite
	{
		void SetPosition(Vector2 position, bool animate = true);
		void SetZoom(float zoom);
		void SetBackgroundColour(Color color);
	}

	public interface ICameraEvents
	{
		event Action<Vector2, float> OnCameraPositionChanged;
		event Action<float, Vector2> OnCameraZoomChanged;
	}
}
