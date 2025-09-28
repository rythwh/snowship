using System.Collections.Generic;

namespace Snowship.NMap.Models.Geography
{
	public class River {
		public NTile.Tile startTile;
		public NTile.Tile centreTile;
		public NTile.Tile endTile;
		public List<NTile.Tile> tiles = new List<NTile.Tile>();
		public int expandRadius;
		public bool ignoreStone;

		public River(NTile.Tile startTile, NTile.Tile centreTile, NTile.Tile endTile, int expandRadius, bool ignoreStone, Map map, bool performPathfinding) {
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
