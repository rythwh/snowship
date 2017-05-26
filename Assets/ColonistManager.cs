using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ColonistManager : MonoBehaviour {

	public List<Colonist> colonists = new List<Colonist>();

	public class Life {
		public float moveSpeed;
		public int health;
		public Life() {

		}
	}

	public class Human:Life {

			public string name;

		public Human() {

		}
	}

	public class Colonist:Human {
		public Colonist() {

		}
	}

	public class Trader:Human {
		public Trader() {

		}
	}

	public void SpawnColonists(int amount) {
		for (int i = 0;i < amount;i++) {
			Colonist colonist = new Colonist();
			colonists.Add(colonist);
		}
	}
}
