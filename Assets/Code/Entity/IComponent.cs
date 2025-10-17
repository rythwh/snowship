namespace Snowship.NEntity
{
	public interface IComponent
	{
		void OnAttach(Entity entity);
		void OnDetach();
	}
}
