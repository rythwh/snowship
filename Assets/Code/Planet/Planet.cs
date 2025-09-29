using System.Collections.Generic;
using Snowship.NMap;

namespace Snowship.NPlanet {
	public class Planet : Map {

		public string directory;
		public string lastSaveDateTime;
		public string lastSaveTimeChunk;

		public string name;
		public List<PlanetTile> planetTiles = new();

		public string regenerationCode;

		public Planet(MapData mapData) : base(mapData) {
		}

		public void SetDirectory(string directory) {
			this.directory = directory;
		}

		public void SetLastSaveDateTime(string lastSaveDateTime, string lastSaveTimeChunk) {
			this.lastSaveDateTime = lastSaveDateTime;
			this.lastSaveTimeChunk = lastSaveTimeChunk;
		}
	}
}
