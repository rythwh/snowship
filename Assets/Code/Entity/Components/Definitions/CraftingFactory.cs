using Snowship.NEntity;

namespace Snowship.NEntity
{
	public class CraftingFactory : IJsonComponentFactory
	{
		public EEntityComponent Id => EEntityComponent.Crafting;

		public IComponent Create(JsonArgs args)
		{
			return new CraftingComponent();
		}
	}
}
