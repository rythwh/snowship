using System;
using Snowship.NHuman;

namespace Snowship.NEntity
{
	public class BedComponent : IComponent
	{
		public float ComfortAmount { get; }
		public Human Occupant { get; private set; }

		public BedComponent(float comfortAmount)
		{
			ComfortAmount = comfortAmount;
		}

		public void SetOccupant(Human occupant)
		{
			if (Occupant != null) {
				throw new InvalidOperationException($"{occupant.Name} tried to occupy Bed with existing occupant {Occupant.Name}");
			}
			Occupant = occupant;
		}

		public void OnDetach()
		{
			if (Occupant != null) {
				throw new InvalidOperationException($"Tried to remove Bed with occupant {Occupant.Name}");
			}
		}
	}
}
