using System.Collections.Generic;
using Snowship.NMap;

namespace Snowship.NPlanet {
	public class Planet : Map {

		public string name;
		public List<PlanetTile> planetTiles = new();

		public Planet(MapData mapData, MapContext mapContext) : base(mapData, mapContext) {
		}
	}
}
