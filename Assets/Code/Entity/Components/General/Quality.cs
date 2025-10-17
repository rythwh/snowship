namespace Snowship.NEntity
{
	public class Quality : IComponent
	{
		public void OnAttach(Entity entity) { }
		public void OnDetach() { }
	}

	public class QualityLevel
	{
		public string Name { get; }
		public int Tier { get; }
	}
}
