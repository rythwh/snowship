using Snowship.NEntity;

namespace Snowship.NMaterial
{
	public class BaseStatsFactory : IJsonComponentFactory
	{
		public string Id => "BaseStats";

		public IComponent Create(JsonArgs args)
		{
			return new BaseStatsC(args.StatMapFromSelf());
		}
	}
}
