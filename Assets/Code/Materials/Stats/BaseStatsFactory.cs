using Snowship.NEntity;

namespace Snowship.NMaterial
{
	public class BaseStatsFactory : IJsonComponentFactory
	{
		public EEntityComponent Id => EEntityComponent.BaseStats;

		public IComponent Create(JsonArgs args)
		{
			return new BaseStatsC(args.StatMapFromSelf());
		}
	}
}
