using Snowship.NMaterial;

namespace Snowship.NEntity
{
	public interface IItemTraitFactory
	{
		ETrait Id { get; }

		IItemTraitBlueprint Parse(JsonArgs args);
	}
}
