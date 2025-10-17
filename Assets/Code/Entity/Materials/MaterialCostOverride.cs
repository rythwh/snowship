using System;
using System.Collections.Generic;

namespace Snowship.NEntity
{
	public class MaterialCostOverride
	{
		public MaterialOverrideMode Mode { get; }
		public IReadOnlyList<Ingredient> Recipe { get; }
		public IReadOnlyList<Ingredient> Add { get; }
		public IReadOnlyList<Ingredient> Remove { get; }

		public MaterialCostOverride(
			MaterialOverrideMode mode,
			IReadOnlyList<Ingredient> recipe,
			IReadOnlyList<Ingredient> add,
			IReadOnlyList<Ingredient> remove
		)
		{
			Mode = mode;
			Recipe = recipe ?? Array.Empty<Ingredient>();
			Add = add ?? Array.Empty<Ingredient>();
			Remove = remove ?? Array.Empty<Ingredient>();
		}
	}
}