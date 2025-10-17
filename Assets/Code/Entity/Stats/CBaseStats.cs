using System.Collections.Generic;

namespace Snowship.NEntity
{
	public class CBaseStats : IComponent
	{
		private readonly Dictionary<EStat, float> baseValues;

		public CBaseStats(Dictionary<EStat, float> baseValues)
		{
			this.baseValues = baseValues;
		}

		public bool TryGetBase(EStat id, out float value)
		{
			return baseValues.TryGetValue(id, out value);
		}

		public void OnAttach(Entity entity) { }
		public void OnDetach() { }
	}
}
