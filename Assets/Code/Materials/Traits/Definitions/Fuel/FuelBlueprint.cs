using Snowship.NEntity;
using VContainer;

namespace Snowship.NMaterial
{
	public class FuelBlueprint : IItemTraitBlueprint
	{
		public float EnergyPerUnit { get; }

		public FuelBlueprint(float energyPerUnit)
		{
			EnergyPerUnit = energyPerUnit;
		}

		public IComponent Create(IObjectResolver resolver)
		{
			return new FuelC(EnergyPerUnit);
		}
	}
}
