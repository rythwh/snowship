using System.Collections.Generic;
using Snowship.NEntity;

namespace Snowship.NMaterial
{
	public class BaseStatsC : IComponent
	{
		private readonly Dictionary<EStat, float> baseValues;

		public BaseStatsC(Dictionary<EStat, float> baseValues)
		{
			this.baseValues = baseValues;
		}

		public bool TryGetBase(EStat id, out float value)
		{
			return baseValues.TryGetValue(id, out value);
		}

		public void OnAttach(NEntity.Entity entity) { }
		public void OnDetach() { }
	}
}
