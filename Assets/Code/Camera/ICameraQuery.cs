using UnityEngine;

namespace Snowship.NCamera
{
	public interface ICameraQuery
	{
		Vector2 CurrentPosition { get; }
		float CurrentZoom { get; }

		Vector3 ScreenToWorld(Vector3 screenPoint);
		Vector3 ScreenToViewport(Vector3 screenPoint);

		Vector3 WorldToScreen(Vector3 worldPosition);
		Vector3 WorldToViewport(Vector3 worldPosition);
	}
}