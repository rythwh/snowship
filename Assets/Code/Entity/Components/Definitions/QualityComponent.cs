using System.Collections.Generic;
using Snowship.NMaterial;

namespace Snowship.NEntity
{
	public class QualityComponent : IComponent, IStatProvider
	{
		public float ValueMultiplier { get; }
		public float BuildWorkMultiplier { get; }

		public QualityComponent(
			float valueMultiplier,
			float buildWorkMultiplier
		)
		{
			ValueMultiplier = valueMultiplier;
			BuildWorkMultiplier = buildWorkMultiplier;
		}

		public void CollectModifiers(List<StatModifier> modifiers)
		{
			modifiers.Add(new StatModifier(EStat.Value, EStatOperation.Multiply, ValueMultiplier));
			modifiers.Add(new StatModifier(EStat.BuildTime, EStatOperation.Multiply, BuildWorkMultiplier));
		}
	}
}
