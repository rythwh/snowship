using VContainer;

namespace Snowship.NEntity
{
	public class BaseStatsFactory : IJsonComponentFactory
	{
		public string TypeId => "BaseStats";

		public IComponent Create(JsonArgs args, IObjectResolver resolver)
		{
			return new CBaseStats(args.StatMapFromSelf());
		}
	}
}
