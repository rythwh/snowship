namespace Snowship.NEntity
{
	public class CAttachedToEntity : ILocation
	{
		public Entity Parent { get; private set; }

		public CAttachedToEntity(Entity parent)
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
