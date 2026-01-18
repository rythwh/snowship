using Snowship.NEntity;
using VContainer;

namespace Snowship.NMaterial
{
	public class FuelFactory : IItemTraitFactory
	{
		public ETrait Id => ETrait.Fuel;

		public IItemTraitBlueprint Parse(JsonArgs args)
		{
			float energy = args.TryGetFloat("energyPerUnit", 0f);
			return new FuelBlueprint(energy);
		}
	}

	public class FuelBlueprint : IItemTraitBlueprint
	{
		public float EnergyPerUnit { get; }

		public FuelBlueprint(float energyPerUnit)
		{
			EnergyPerUnit = energyPerUnit;
		}

		public IComponent Create(IObjectResolver resolver)
		{
			return new FuelComponent(EnergyPerUnit);
		}
	}

	public class FuelComponent : IComponent
	{
		public float EnergyPerUnit { get; }

		public FuelComponent(float energyPerUnit)
		{
			EnergyPerUnit = energyPerUnit;
		}

		public void OnAttach(Entity entity) { }
		public void OnDetach() { }
	}
}
