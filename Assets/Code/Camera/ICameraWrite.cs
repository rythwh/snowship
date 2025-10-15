using UnityEngine;

namespace Snowship.NCamera
{
	public interface ICameraWrite
	{
		void SetPosition(Vector2 position, float time = 0);
		void SetZoom(float zoom, float time = 0);
		void SetBackgroundColour(Color color);
	}
}