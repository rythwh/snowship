using System.Collections.Generic;
using System.Linq;
using Snowship.NMap.Tile;

namespace Snowship.NMap.Models.Structure
{
	public class Region {

		public TileType tileType;
		public List<Tile.Tile> tiles = new List<Tile.Tile>();
		public int id;

		public List<Region> connectedRegions = new List<Region>();

		public bool Visible { get; private set; }

		private MapManager MapManager => GameManager.Get<MapManager>();

		public Region(TileType regionTileType, int regionID) {
			tileType = regionTileType;
			id = regionID;
		}

		public void SetVisible(bool visible, bool retile, bool recalculateLighting) {
			Visible = visible;

			List<Tile.Tile> tilesToModify = new List<Tile.Tile>();
			foreach (Tile.Tile tile in tiles) {
				tile.SetVisible(Visible);

				tilesToModify.Add(tile);
				tilesToModify.AddRange(tile.surroundingTiles);
			}

			tilesToModify = tilesToModify.Distinct().ToList();

			if (retile) {
				MapManager.Map.Bitmasking(tilesToModify, true, false);
			}

			if (recalculateLighting) {
				MapManager.Map.RecalculateLighting(tilesToModify, true);
			}

		}
	}
}
