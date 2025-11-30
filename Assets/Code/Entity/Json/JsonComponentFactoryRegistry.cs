using System;
using System.Collections.Generic;

namespace Snowship.NEntity
{
	public sealed class JsonComponentFactoryRegistry
	{
		private readonly Dictionary<string, IJsonComponentFactory> idToFactory;

		public JsonComponentFactoryRegistry(IEnumerable<IJsonComponentFactory> factories)
		{
			idToFactory = new Dictionary<string, IJsonComponentFactory>(StringComparer.OrdinalIgnoreCase);
			foreach (IJsonComponentFactory factory in factories) {
				idToFactory[factory.Id] = factory;
			}
		}

		public bool TryGet(string typeId, out IJsonComponentFactory factory)
		{
			return idToFactory.TryGetValue(typeId, out factory);
		}
	}
}
