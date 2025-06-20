﻿using Snowship.NPersistence;
using Snowship.NUtilities;

namespace Snowship.NColony {
	public class Colony : ILocation {
		public string directory;
		public string lastSaveDateTime;
		public string lastSaveTimeChunk;

		public string Name { get; }

		public MapData mapData;
		public Map map;

		public Colony(string name, MapData mapData) {
			Name = name;
			this.mapData = mapData;

			lastSaveDateTime = PersistenceUtilities.GenerateSaveDateTimeString();
			lastSaveTimeChunk = PersistenceUtilities.GenerateDateTimeString();
		}

		protected Colony() {
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
