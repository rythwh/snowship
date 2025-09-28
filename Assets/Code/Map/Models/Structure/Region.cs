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

		private MapManager MapManager => GameManager.Get<MapManager>();

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
				MapManager.Map.Bitmasking(tilesToModify, true, false);
			}

			if (recalculateLighting) {
				MapManager.Map.RecalculateLighting(tilesToModify, true);
			}

		}
	}
}
