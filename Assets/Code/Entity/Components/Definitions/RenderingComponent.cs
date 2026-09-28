namespace Snowship.NEntity
{
	public class RenderingComponent : IComponent
	{
		public string SpritePath { get; }
		public float PivotX { get; }
		public float PivotY { get; }

		public RenderingComponent(string spritePath, float pivotX = 0.5f, float pivotY = 0.5f)
		{
			SpritePath = spritePath;
			PivotX = pivotX;
			PivotY = pivotY;
		}
	}
}
