namespace Snowship.NEntity
{
	public interface ICostProvider
	{
		void ApplyTo(Entity entity, CostDraft draft);
	}
}
