using Snowship.NEntity;

namespace Snowship
{
	public interface ICostProvider
	{
		void ApplyTo(Entity entity, CostDraft draft);
	}
}
