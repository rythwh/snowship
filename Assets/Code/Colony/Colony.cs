using Snowship.NUtilities;

namespace Snowship.NColony {
	public class Colony : ILocation {

		public string Name { get; }

		public Colony(string name) {
			Name = name;
		}
	}
}
