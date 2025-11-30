using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Snowship.NEntity
{
	public class BaseRecipeFactory : IJsonComponentFactory
	{
		public string Id => "BaseRecipe";

		public IComponent Create(JsonArgs args)
		{
			List<Ingredient> ingredients = args.DataToken is JArray array
				? JsonArgs.ReadIngredientsArray(array)
				: new List<Ingredient>();
			return new BaseRecipeC(ingredients);
		}
	}
}
