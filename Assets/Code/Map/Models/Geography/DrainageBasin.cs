using System.Collections.Generic;
using Snowship.NMap.Models.Structure;
using Snowship.NMap.NTile;

namespace Snowship.NMap.Models.Geography
{
	public class DrainageBasin
	{
		public Tile HighestTile { get; }
		public Region Region { get; }

		public List<Tile> Tiles => Region.tiles;

		public DrainageBasin(Tile highestTile, Region region) {
			HighestTile = highestTile;
			Region = region;
		}
	}
}
