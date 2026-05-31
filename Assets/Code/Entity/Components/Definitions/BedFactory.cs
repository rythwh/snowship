using Snowship.NEntity;

namespace Snowship.NEntity
{
	public class BedFactory : IJsonComponentFactory
	{
		public EEntityComponent Id => EEntityComponent.Bed;

		public IComponent Create(JsonArgs args)
		{
			float comfortAmount = args.GetFloatOrDefault("comfort_amount", 0f);
			return new BedComponent(comfortAmount);
		}
	}
}
