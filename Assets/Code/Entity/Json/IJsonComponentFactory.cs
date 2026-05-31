namespace Snowship.NEntity
{
	public interface IJsonComponentFactory
	{
		EEntityComponent Id { get; }
		IComponent Create(JsonArgs args);
	}
}
