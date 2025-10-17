using System.Collections.Generic;

namespace Snowship.NEntity
{
	public class CQuality : IComponent, IStatProvider
	{
		public float ValueMultiplier { get; }
		public float BuildWorkMultiplier { get; }

		public CQuality(
			float valueMultiplier,
			float buildWorkMultiplier
		)
		{
			ValueMultiplier = valueMultiplier;
			BuildWorkMultiplier = buildWorkMultiplier;
		}

		public void CollectModifiers(List<StatModifier> modifiers)
		{
			modifiers.Add(new StatModifier(EStat.Value, EStatOp.Mul, ValueMultiplier));
			modifiers.Add(new StatModifier(EStat.BuildTime, EStatOp.Mul, BuildWorkMultiplier));
		}

		public void OnAttach(Entity entity) { }
		public void OnDetach() { }
	}
}
