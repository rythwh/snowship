using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.NTile;

namespace Snowship.NMap.Models.Structure
{
	public class Region
	{
		public TileType tileType;
		public List<Tile> tiles = new List<Tile>();

		public List<Region> connectedRegions = new List<Region>();

		public bool Visible { get; private set; }

		private IMapQuery MapQuery => GameManager.Get<IMapQuery>();

		public Region(TileType regionTileType) {
			tileType = regionTileType;
		}

		public void SetVisible(bool visible, bool retile, bool recalculateLighting) {
			Visible = visible;

			List<Tile> tilesToModify = new List<Tile>();
			foreach (Tile tile in tiles) {
				tile.SetVisible(Visible);

				tilesToModify.Add(tile);
				tilesToModify.AddRange(tile.surroundingTiles);
			}

			tilesToModify = tilesToModify.Distinct().ToList();

			if (retile) {
				MapQuery.Map.RedrawTiles(tilesToModify, true, false);
			}

			if (recalculateLighting) {
				MapQuery.Map.RecalculateLighting(tilesToModify, true);
			}

		}
	}
}
