using Snowship.NMaterial;
using VContainer;

namespace Snowship.NEntity
{
	public interface IItemTraitFactory
	{
		ETrait Id { get; }

		IItemTraitBlueprint Parse(JsonArgs args);
	}
}
