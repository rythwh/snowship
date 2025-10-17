using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using VContainer;

namespace Snowship.NEntity
{
	public class MaterialOptionsFactory : IJsonComponentFactory
	{
		public string TypeId => "MaterialOptions";

		public IComponent Create(JsonArgs args, IObjectResolver resolver)
		{
			// allowed.classes, allowed.materials
			IReadOnlyList<string> classes = args.PathStringList("allowed", "classes");
			IReadOnlyList<string> materials = args.PathStringList("allowed", "materials");

			// defaults.class or defaults.material
			string defClass = args.PathStr("defaults", "class", null);
			string defMaterial = args.PathStr("defaults", "material", null);

			// cost.byClass, cost.byMaterial
			Dictionary<string, MaterialCostOverride> byClass = ParseOverrideMap(args, "cost", "byClass");
			Dictionary<string, MaterialCostOverride> byMaterial = ParseOverrideMap(args, "cost", "byMaterial");

			// Sensible default if nothing is set
			if (string.IsNullOrWhiteSpace(defClass) && string.IsNullOrWhiteSpace(defMaterial)) {
				if (materials.Count > 0) {
					defMaterial = materials[0];
				} else if (classes.Count > 0) {
					defClass = classes[0];
				}
			}

			return new MaterialOptions(classes, materials, defClass, defMaterial, byClass, byMaterial);
		}

		private static Dictionary<string, MaterialCostOverride> ParseOverrideMap(JsonArgs args, string rootKey, string subKey)
		{
			JObject map = args.PathObject(rootKey, subKey);
			if (map == null) {
				return new Dictionary<string, MaterialCostOverride>(StringComparer.OrdinalIgnoreCase);
			}

			Dictionary<string, MaterialCostOverride> result = new Dictionary<string, MaterialCostOverride>(StringComparer.OrdinalIgnoreCase);
			foreach (JProperty p in map.Properties()) {
				JObject ov = (JObject)p.Value;
				string modeString = ov.TryGetValue("mode", out JToken modeToken) ? modeToken.Value<string>() : "ReplaceAll";
				MaterialOverrideMode mode = (MaterialOverrideMode)Enum.Parse(typeof(MaterialOverrideMode), modeString, true);
				List<Ingredient> recipe = null;
				if (ov.TryGetValue("recipe", out JToken recipeToken) && recipeToken.Type == JTokenType.Array) {
					recipe = JsonArgs.ReadIngredientsArray((JArray)recipeToken);
				}

				List<Ingredient> add = null;
				if (ov.TryGetValue("add", out JToken addToken) && addToken.Type == JTokenType.Array) {
					add = JsonArgs.ReadIngredientsArray((JArray)addToken);
				}

				List<Ingredient> remove = null;
				if (ov.TryGetValue("remove", out JToken removeToken) && removeToken.Type == JTokenType.Array) {
					remove = JsonArgs.ReadIngredientsArray((JArray)removeToken);
				}

				result[p.Name] = new MaterialCostOverride(mode, recipe, add, remove);
			}

			return result;
		}
	}
}
