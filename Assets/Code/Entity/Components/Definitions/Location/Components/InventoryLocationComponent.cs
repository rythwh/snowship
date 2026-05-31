using Snowship.NResource.NInventory;

namespace Snowship.NEntity
{
	public class InventoryLocationComponent : ILocation
	{
		public Inventory Inventory { get; private set; }

		public InventoryLocationComponent(Inventory inventory)
		{
			Inventory = inventory;
		}

		public void OnAttach(Entity entity) { }
		public void OnDetach() { }
	}
}
