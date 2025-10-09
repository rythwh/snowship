using Snowship.NMap.Models.Structure;

namespace Snowship.NPath
{
	internal class PathfindingRegionBlock {
		public RegionBlock regionBlock;
		public PathfindingRegionBlock cameFrom;
		public float cost;

		public PathfindingRegionBlock(RegionBlock regionBlock, PathfindingRegionBlock cameFrom, float cost) {
			this.regionBlock = regionBlock;
			this.cameFrom = cameFrom;
			this.cost = cost;
		}
	}
}
