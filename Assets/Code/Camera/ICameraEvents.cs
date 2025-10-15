using System;
using UnityEngine;

namespace Snowship.NCamera
{
	public interface ICameraEvents
	{
		event Action<Vector2, float> OnCameraPositionChanged;
		event Action<float, Vector2> OnCameraZoomChanged;
	}
}