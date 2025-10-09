using Snowship.NMap.NTile;

namespace Snowship.NPath
{
	internal class PathfindingTile
	{
		public Tile tile;
		public PathfindingTile cameFrom;
		public int pathDistance = 0;
		public float cost;

		public PathfindingTile(Tile tile, PathfindingTile cameFrom, float cost) {
			this.tile = tile;
			this.cameFrom = cameFrom;
			this.cost = cost;
			if (cameFrom != null) {
				pathDistance = cameFrom.pathDistance + 1;
			} else {
				pathDistance = 0;
			}
		}
	}
}
