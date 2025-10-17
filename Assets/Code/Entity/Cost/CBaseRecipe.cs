using System.Collections.Generic;
using Snowship.NEntity;

namespace Snowship
{
	public class CBaseRecipe : IComponent, ICostProvider
	{
		private readonly List<Ingredient> baseIngredients;

		public CBaseRecipe(IEnumerable<Ingredient> ingredients)
		{
			baseIngredients = new List<Ingredient>(ingredients);
		}

		public void ApplyTo(Entity entity, CostDraft draft)
		{
			draft.ReplaceAll(baseIngredients);
		}

		public void OnAttach(Entity entity) { }
		public void OnDetach() { }
	}
}
