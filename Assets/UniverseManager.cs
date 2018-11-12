using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniverseManager : BaseManager {

	public Universe universe = null;

	public Universe CreateUniverse(string name) {
		Universe universe = new Universe(name);

		this.universe = universe;

		GameManager.persistenceM.CreateUniverse(universe);

		return universe;
	}

	public void SetUniverse(Universe universe) {
		this.universe = universe;
	}

	public class Universe {
		public string name;
		public string directory;
		public string lastSaveDateTime;

		public Universe(string name) {
			this.name = name;

			lastSaveDateTime = PersistenceManager.GenerateSaveDateTimeString();
		}

		public void SetDirectory(string directory) {
			this.directory = directory;
		}

		public void SetLastSaveDateTime(string lastSaveDateTime) {
			this.lastSaveDateTime = lastSaveDateTime;
		}
	}
}