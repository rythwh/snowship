using Snowship.NEntity;

namespace Snowship.NMaterial
{
	public class FuelC : IComponent
	{
		public float EnergyPerUnit { get; }

		public FuelC(float energyPerUnit)
		{
			EnergyPerUnit = energyPerUnit;
		}

		public void OnAttach(NEntity.Entity entity) { }
		public void OnDetach() { }
	}
}
