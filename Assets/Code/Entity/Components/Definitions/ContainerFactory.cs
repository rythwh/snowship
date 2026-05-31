using Snowship.NEntity;

namespace Snowship.NEntity
{
	public class ContainerFactory : IJsonComponentFactory
	{
		public EEntityComponent Id => EEntityComponent.Container;

		public IComponent Create(JsonArgs args)
		{
			int weight = args.GetIntOrDefault("weight", 0);
			int volume = args.GetIntOrDefault("volume", 0);
			return new ContainerComponent(weight, volume);
		}
	}
}
