using System.Collections.Generic;

namespace Snowship.NMaterial
{
	public class StatService
	{
		private readonly List<StatModifier> buffer = new();

		public bool TryGet(NEntity.Entity entity, EStat stat, out float result)
		{
			float baseValue = 0f;
			bool hasBase = false;

			if (entity.TryGet(out BaseStatsC baseStats)) {
				if (baseStats.TryGetBase(stat, out float value)) {
					baseValue = value;
					hasBase = true;
				}
			}

			buffer.Clear();
			foreach (IStatProvider statProvider in entity.StatProviders) {
				statProvider.CollectModifiers(buffer);
			}

			float addSum = 0f;
			float mulProduct = 1f;

			foreach (StatModifier modifier in buffer) {
				if (modifier.Id != stat) {
					continue;
				}

				if (modifier.Operation == EStatOperation.Add) {
					addSum += modifier.Value;
				} else {
					mulProduct *= modifier.Value;
				}
			}

			if (!hasBase) {
				result = addSum * mulProduct;
				return buffer.Count > 0;
			}

			result = (baseValue + addSum) * mulProduct;
			return true;
		}
	}
}
