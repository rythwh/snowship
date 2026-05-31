namespace Snowship.NEntity
{
	public class AttachedToEntityComponent : ILocation
	{
		public Entity Parent { get; private set; }

		public AttachedToEntityComponent(Entity parent)
		{
			Parent = parent;
		}

		public void SetParent(Entity parent)
		{
			Parent = parent;
		}

		public void OnAttach(Entity entity) { }
		public void OnDetach() { }
	}
}
