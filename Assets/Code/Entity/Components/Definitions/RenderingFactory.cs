namespace Snowship.NEntity
{
	public class RenderingFactory : IJsonComponentFactory
	{
		public EEntityComponent Id => EEntityComponent.Rendering;

		public IComponent Create(JsonArgs args)
		{
			string spritePath = args.GetStringOrDefault("sprite_path", string.Empty);
			float pivotX = args.GetFloatOrDefault("pivot_x", 0.5f);
			float pivotY = args.GetFloatOrDefault("pivot_y", 0.5f);
			return new RenderingComponent(spritePath, pivotX, pivotY);
		}
	}
}
