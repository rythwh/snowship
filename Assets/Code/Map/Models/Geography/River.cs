using System.Collections.Generic;
using JetBrains.Annotations;
using Snowship.NMap.NTile;

namespace Snowship.NMap.Models.Geography
{
	public class River {
		public Tile startTile;
		public Tile centreTile;
		public Tile endTile;
		public List<Tile> tiles;
		public int expandRadius;
		public bool ignoreStone;

		public River(List<Tile> tiles, int expandRadius, bool ignoreStone) {
			this.tiles = tiles;
			if (tiles.Count > 0) {
				startTile = tiles[0];
				endTile = tiles[^1];
			}
			this.expandRadius = expandRadius;
			this.ignoreStone = ignoreStone;
		}

		public static River GetRiverContainingTile([NotNull] Tile tile, [NotNull] params List<River>[] riverGroups) {

			if (riverGroups is not { Length: > 0 }) {
				return null;
			}

			List<River> riversToCheck = new();
			foreach (List<River> river in riverGroups) {
				riversToCheck.AddRange(river);
			}

			foreach (River river in riversToCheck) {
				foreach (Tile riverTile in river.tiles) {
					if (riverTile == tile) {
						return river;
					}
				}
			}

			return null;
		}

		public static bool DoAnyRiversContainTile([NotNull] Tile tile, [NotNull] params List<River>[] riverGroups) {
			return GetRiverContainingTile(tile, riverGroups) != null;
		}
	}
}
