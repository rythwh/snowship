using System.Collections.Generic;

namespace Snowship.NEntity
{
	public class MaterialDef
	{
		public string Id { get; }
		public string ClassId { get; }
		public IReadOnlyList<StatModifier> Modifiers { get; }

		public MaterialDef(
			string id,
			string classId,
			IReadOnlyList<StatModifier> modifiers
		)
		{
			Id = id;
			ClassId = classId;
			Modifiers = modifiers;
		}
	}
}
