using Snowship.NEntity;

namespace Snowship.NEntity
{
	public class QualityFactory : IJsonComponentFactory
	{
		public EEntityComponent Id => EEntityComponent.Quality;

		public IComponent Create(JsonArgs args)
		{
			float valueMultiplier = args.GetFloatOrDefault("value_multiplier", 1f);
			float buildWorkMultiplier = args.GetFloatOrDefault("build_work_multiplier", 1f);
			return new QualityComponent(valueMultiplier, buildWorkMultiplier);
		}
	}
}
