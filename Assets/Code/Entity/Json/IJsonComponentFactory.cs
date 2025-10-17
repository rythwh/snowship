using VContainer;

namespace Snowship.NEntity
{
	public interface IJsonComponentFactory
	{
		string TypeId { get; }
		IComponent Create(JsonArgs args, IObjectResolver resolver);
	}
}
