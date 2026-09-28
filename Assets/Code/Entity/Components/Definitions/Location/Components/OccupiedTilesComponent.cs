using System.Collections.Generic;
using Snowship.NMap.NTile;

namespace Snowship.NEntity
{
	public class OccupiedTilesComponent : IComponent
	{
		public List<Tile> Tiles { get; } = new();

		public OccupiedTilesComponent(IEnumerable<Tile> tiles)
		{
			Tiles.AddRange(tiles);
		}
	}
}
