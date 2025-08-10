

using Snowship.NPersistence;

public class UniverseManager : IManager {

	public Universe universe = null;

	public static string GetRandomUniverseName() {
		//return GameManager.Get<ResourceManager>().GetRandomLocationName();
		return "Universe " + System.DateTime.Today.ToString("ddMMyyyy");
	}

	public Universe CreateUniverse(string name) {
		Universe universe = new Universe(name);

		this.universe = universe;

		return universe;
	}

	public void SetUniverse(Universe universe) {
		this.universe = universe;
	}

	public class Universe {

		public string name;

		public Universe(string name) {
			this.name = name;
		}
	}
}
