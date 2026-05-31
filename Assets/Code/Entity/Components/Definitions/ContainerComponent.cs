using System;
using Snowship.NResource.NInventory;

namespace Snowship.NEntity
{
	public class ContainerComponent : IComponent, IInventoriable
	{
		public Inventory Inventory { get; private set; }

		public ContainerComponent(int weight, int volume)
		{
			Inventory = new Inventory(this, weight, volume);
		}

		public void OnDetach()
		{
			if (Inventory.UsedVolume() > 0 || Inventory.UsedWeight() > 0) {
				throw new InvalidOperationException($"Trying to remove container with resources still inside.");
			}
		}
	}
}
