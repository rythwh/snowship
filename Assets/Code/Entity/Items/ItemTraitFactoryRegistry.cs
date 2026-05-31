using System.Collections.Generic;
using Snowship.NMaterial;

namespace Snowship.NEntity
{
	public class ItemTraitFactoryRegistry
	{
		private readonly Dictionary<ETrait, IItemTraitFactory> idToFactory;

		public ItemTraitFactoryRegistry(IEnumerable<IItemTraitFactory> factories)
		{
			idToFactory = new Dictionary<ETrait, IItemTraitFactory>();
			foreach (IItemTraitFactory factory in factories) {
				idToFactory[factory.Id] = factory;
			}
		}

		public bool TryGet(ETrait id, out IItemTraitFactory factory)
		{
			return idToFactory.TryGetValue(id, out factory);
		}
	}
}
