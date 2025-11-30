using VContainer;

namespace Snowship.NEntity
{
	public interface IItemTraitBlueprint
	{
		IComponent Create(IObjectResolver resolver);
	}
}
