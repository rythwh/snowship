using Snowship.NResource.NInventory;

namespace Snowship.NEntity
{
	public class InventoryLocationC : ILocation
	{
		public Inventory Inventory { get; private set; }

		public InventoryLocationC(Inventory inventory)
		{
			Inventory = inventory;
		}

		public void OnAttach(Entity entity) { }
		public void OnDetach() { }
	}
}
