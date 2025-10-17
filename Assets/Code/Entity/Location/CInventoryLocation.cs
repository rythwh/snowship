using Snowship.NResource.NInventory;

namespace Snowship.NEntity
{
	public class CInventoryLocation : ILocation
	{
		public Inventory Inventory { get; private set; }

		public CInventoryLocation(Inventory inventory)
		{
			Inventory = inventory;
		}

		public void OnAttach(Entity entity) { }
		public void OnDetach() { }
	}
}
