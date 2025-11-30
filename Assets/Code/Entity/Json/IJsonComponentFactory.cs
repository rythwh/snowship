namespace Snowship.NEntity
{
	public interface IJsonComponentFactory
	{
		string Id { get; }
		IComponent Create(JsonArgs args);
	}
}
