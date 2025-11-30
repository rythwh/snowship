using System.Collections.Generic;
using Snowship.NEntity;

namespace Snowship.NMaterial
{
	public class Material
	{
		public string Id { get; }
		public string ClassId { get; }
		public int Weight { get; }
		public int Volume { get; }
		public int Value { get; }
		public IReadOnlyList<StatModifier> Modifiers { get; }
		public IReadOnlyList<IItemTraitBlueprint> Traits { get; }

		public Material(
			string id,
			string classId,
			int weight,
			int volume,
			int value,
			IReadOnlyList<StatModifier> modifiers,
			IReadOnlyList<IItemTraitBlueprint> traits
		)
		{
			Id = id;
			ClassId = classId;
			Weight = weight;
			Volume = volume;
			Value = value;
			Modifiers = modifiers ?? new List<StatModifier>();
			Traits = traits ?? new List<IItemTraitBlueprint>();
		}
	}
}
