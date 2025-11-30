namespace Snowship.NEntity
{
	public class AttachedToEntityC : ILocation
	{
		public Entity Parent { get; private set; }

		public AttachedToEntityC(Entity parent)
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
