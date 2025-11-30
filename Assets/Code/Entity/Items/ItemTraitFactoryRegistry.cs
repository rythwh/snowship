using System;
using System.Collections.Generic;
using Snowship.NMaterial;

namespace Snowship.NEntity
{
	public class ItemTraitFactoryRegistry
	{
		private readonly Dictionary<ETrait, IItemTraitFactory> byType;

		public ItemTraitFactoryRegistry(IEnumerable<IItemTraitFactory> factories)
		{
			byType = new Dictionary<ETrait, IItemTraitFactory>();
			foreach (IItemTraitFactory factory in factories) {
				byType[factory.Id] = factory;
			}
		}

		public bool TryGet(ETrait id, out IItemTraitFactory factory)
		{
			return byType.TryGetValue(id, out factory);
		}
	}
}
