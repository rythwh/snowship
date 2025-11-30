using System.Collections.Generic;
using Snowship.NMaterial;

namespace Snowship.NEntity
{
	public class QualityC : IComponent, IStatProvider
	{
		public float ValueMultiplier { get; }
		public float BuildWorkMultiplier { get; }

		public QualityC(
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

		public void OnAttach(Entity entity) { }
		public void OnDetach() { }
	}
}
