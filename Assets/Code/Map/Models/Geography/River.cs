using System.Collections.Generic;

namespace Snowship.NMap.Models.Geography
{
	public class River {
		public Tile.Tile startTile;
		public Tile.Tile centreTile;
		public Tile.Tile endTile;
		public List<Tile.Tile> tiles = new List<Tile.Tile>();
		public int expandRadius;
		public bool ignoreStone;

		public River(Tile.Tile startTile, Tile.Tile centreTile, Tile.Tile endTile, int expandRadius, bool ignoreStone, Map map, bool performPathfinding) {
			this.startTile = startTile;
			this.centreTile = centreTile;
			this.endTile = endTile;
			this.expandRadius = expandRadius;
			this.ignoreStone = ignoreStone;

			if (performPathfinding) {
				if (centreTile != null) {
					tiles.AddRange(map.RiverPathfinding(startTile, centreTile, expandRadius, ignoreStone));
					tiles.AddRange(map.RiverPathfinding(centreTile, endTile, expandRadius, ignoreStone));
				} else {
					tiles = map.RiverPathfinding(startTile, endTile, expandRadius, ignoreStone);
				}
			}
		}

		protected River() {
		}
	}
}
