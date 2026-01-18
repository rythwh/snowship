using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Snowship.NEntity;
using VContainer;

namespace Snowship.NMaterial
{
	public class CraftableFactory : IItemTraitFactory
	{
		public ETrait Id => ETrait.Craftable;

		public IItemTraitBlueprint Parse(JsonArgs args)
		{
			JArray materialAmountArray = args.TryGetArray("materialsToCraft");
			List<JsonMaterialAmount> materialsToCraft = materialAmountArray != null
				? materialAmountArray.ToObject<List<JsonMaterialAmount>>()
				: new List<JsonMaterialAmount>();

			return new CraftableBlueprint(
				args.TryGetString("craftWithEntities", string.Empty),
				args.TryGetInt("energyRequired", 0),
				materialsToCraft,
				args.TryGetInt("time", 0),
				args.TryGetInt("output", 0),
				args.TryGetBool("activeCrafting", true)
			);
		}
	}

	public class CraftableBlueprint : IItemTraitBlueprint
	{
		public string CraftWithEntities { get; }
		public int EnergyRequired { get; }
		public List<JsonMaterialAmount> MaterialsToCraft { get; }
		public int Time { get; }
		public int Output { get; }
		public bool ActiveCrafting { get; }

		public CraftableBlueprint(
			string craftWithEntities,
			int energyRequired,
			List<JsonMaterialAmount> materialsToCraft,
			int time,
			int output,
			bool activeCrafting
		) {
			CraftWithEntities = craftWithEntities;
			EnergyRequired = energyRequired;
			MaterialsToCraft = materialsToCraft;
			Time = time;
			Output = output;
			ActiveCrafting = activeCrafting;
		}

		public IComponent Create(IObjectResolver resolver)
		{
			MaterialRegistry materialRegistry = resolver.Resolve<IMaterialProvider>().Registry;

			return new CraftableComponent(
				new List<Entity>(), // TODO Get list of entities by name/id
				EnergyRequired,
				MaterialsToCraft.Select(jma => new MaterialAmount(materialRegistry.Get(jma.Id), jma.Amount)).ToList(),
				Time,
				Output,
				ActiveCrafting
			);
		}
	}

	public class CraftableComponent : IComponent
	{
		public List<Entity> CraftWithEntities { get; }
		public int EnergyRequired { get; }
		public List<MaterialAmount> MaterialsToCraft { get; }
		public int Time { get; }
		public int Output { get; }
		public bool ActiveCrafting { get; }

		public CraftableComponent(
			List<Entity> craftWithEntities,
			int energyRequired,
			List<MaterialAmount> materialsToCraft,
			int time,
			int output,
			bool activeCrafting
		) {
			CraftWithEntities = craftWithEntities;
			EnergyRequired = energyRequired;
			MaterialsToCraft = materialsToCraft;
			Time = time;
			Output = output;
			ActiveCrafting = activeCrafting;
		}

		public void OnAttach(Entity entity) { }
		public void OnDetach() { }
	}
}
