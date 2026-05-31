using System.Collections.Generic;

namespace Snowship.NEntity
{
	public sealed class JsonComponentFactoryRegistry
	{
		private readonly Dictionary<EEntityComponent, IJsonComponentFactory> idToFactory;

		public JsonComponentFactoryRegistry(IEnumerable<IJsonComponentFactory> factories)
		{
			idToFactory = new Dictionary<EEntityComponent, IJsonComponentFactory>();
			foreach (IJsonComponentFactory factory in factories) {
				idToFactory[factory.Id] = factory;
			}
		}

		public bool TryGet(EEntityComponent typeId, out IJsonComponentFactory factory)
		{
			return idToFactory.TryGetValue(typeId, out factory);
		}
	}
}
