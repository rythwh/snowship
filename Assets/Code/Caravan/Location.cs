using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NUtilities;

namespace Snowship.NCaravan {
	public class Location : ILocation {

		public static readonly List<string> Wealth = new List<string>() {
			"Destitute", "Poor", "Comfortable", "Wealthy"
		};

		public static readonly List<string> ResourceRichness = new List<string>() {
			"Sparse", "Average", "Abundant"
		};

		public static readonly List<string> CitySize = new List<string>() {
			"Hamlet", "Village", "Town", "City"
		};

		public string Name { get; }
		public readonly string wealth;
		public readonly string resourceRichness;
		public readonly string citySize;
		public readonly Biome.TypeEnum biomeType;

		public Location(
			string name,
			string wealth,
			string resourceRichness,
			string citySize,
			Biome.TypeEnum biomeType
		) {
			Name = name;
			this.wealth = wealth;
			this.resourceRichness = resourceRichness;
			this.citySize = citySize;
			this.biomeType = biomeType;
		}

		public static Location GenerateLocation() {

			string name = GameManager.Get<ResourceManager>().GetRandomLocationName();

			string wealth = Wealth[UnityEngine.Random.Range(0, Wealth.Count)];
			string resourceRichness = ResourceRichness[UnityEngine.Random.Range(0, ResourceRichness.Count)];
			string citySize = CitySize[UnityEngine.Random.Range(0, CitySize.Count)];

			List<Biome.TypeEnum> biomeTypes = ((Biome.TypeEnum[])Enum.GetValues(typeof(Biome.TypeEnum))).ToList();
			Biome.TypeEnum biomeType = biomeTypes[UnityEngine.Random.Range(0, biomeTypes.Count)];

			return new Location(name, wealth, resourceRichness, citySize, biomeType);
		}
	}
}
