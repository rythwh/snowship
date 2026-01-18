using System;
using System.Collections.Generic;
using Snowship.NEntity;

namespace Snowship.NMaterial
{
	public class JsonMaterialLoader
	{
		private readonly ItemTraitFactoryRegistry traitFactories;

		public JsonMaterialLoader(ItemTraitFactoryRegistry traitFactories)
		{
			this.traitFactories = traitFactories;
		}

		public Material JsonToMaterial(JsonMaterial jsonMaterial)
		{
			List<StatModifier> statModifiers = ParseModifiers(jsonMaterial.Modifiers);
			List<IItemTraitBlueprint> traitBlueprints = ParseTraits(jsonMaterial.Traits);

			return new Material(
				jsonMaterial.Id,
				jsonMaterial.ClassId,
				jsonMaterial.Weight,
				jsonMaterial.Volume,
				jsonMaterial.Value,
				statModifiers,
				traitBlueprints
			);
		}

		private List<StatModifier> ParseModifiers(List<JsonMaterialStatMod> modifiersJson)
		{
			List<StatModifier> statModifiers = new();
			if (modifiersJson == null) {
				return statModifiers;
			}

			foreach (JsonMaterialStatMod modifier in modifiersJson) {
				statModifiers.Add(new StatModifier(modifier.Stat, modifier.Operation, modifier.Value));
			}
			return statModifiers;
		}

		private List<IItemTraitBlueprint> ParseTraits(List<JsonMaterialTrait> traitsJson)
		{
			List<IItemTraitBlueprint> blueprints = new();
			if (traitsJson == null) {
				return blueprints;
			}
			foreach (JsonMaterialTrait trait in traitsJson) {
				if (!traitFactories.TryGet(trait.Type, out IItemTraitFactory factory)) {
					throw new NotImplementedException($"IItemTraitFactory does not exist for trait {trait.Type}");
				}
				IItemTraitBlueprint blueprint = factory.Parse(new JsonArgs(trait.Data));
				blueprints.Add(blueprint);
			}
			return blueprints;
		}
	}
}
