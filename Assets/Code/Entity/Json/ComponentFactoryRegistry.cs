using System;
using System.Collections.Generic;

namespace Snowship.NEntity
{
	public sealed class ComponentFactoryRegistry
	{
		private readonly Dictionary<string, IJsonComponentFactory> byType;

		public ComponentFactoryRegistry(IEnumerable<IJsonComponentFactory> factories)
		{
			byType = new Dictionary<string, IJsonComponentFactory>(StringComparer.OrdinalIgnoreCase);
			foreach (IJsonComponentFactory factory in factories) {
				byType[factory.TypeId] = factory;
			}
		}

		public bool TryGet(string typeId, out IJsonComponentFactory factory)
		{
			return byType.TryGetValue(typeId, out factory);
		}
	}
}
